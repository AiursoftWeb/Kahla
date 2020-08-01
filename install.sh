
enable_bbr()
{
    enable_bbr_force()
    {
        echo "BBR not enabled. Enabling BBR..."
        echo 'net.core.default_qdisc=fq' | tee -a /etc/sysctl.conf
        echo 'net.ipv4.tcp_congestion_control=bbr' | tee -a /etc/sysctl.conf
        sysctl -p
    }
    sysctl net.ipv4.tcp_available_congestion_control | grep -q bbr ||  enable_bbr_force
}

set_production()
{
    cat /etc/environment | grep -q "Production" || echo 'ASPNETCORE_ENVIRONMENT="Production"' | tee -a /etc/environment
    export ASPNETCORE_ENVIRONMENT="Production"
}

get_port()
{
    while true; 
    do
        local PORT=$(shuf -i 40000-65000 -n 1)
        ss -lpn | grep -q ":$PORT " || echo $PORT && break
    done
}

open_port()
{
    port_to_open="$1"
    if [[ "$port_to_open" == "" ]]; then
        echo "You must specify a port!'"
        return 9
    fi

    ufw allow $port_to_open/tcp
    ufw reload
}

enable_firewall()
{
    open_port 22
    echo "y" | ufw enable
    echo "Firewall enabled!"
    ufw status
}

add_caddy_proxy()
{
    domain_name="$1"
    local_port="$2"
    cat /etc/caddy/Caddyfile | grep -q "an easy way" && echo "" > /etc/caddy/Caddyfile
    echo "
$domain_name {
    reverse_proxy /* 127.0.0.1:$local_port
}" >> /etc/caddy/Caddyfile
    systemctl restart caddy.service
}

register_service()
{
    service_name="$1"
    local_port="$2"
    run_path="$3"
    dll="$4"
    echo "[Unit]
    Description=$dll Service
    After=network.target
    Wants=network.target

    [Service]
    Type=simple
    ExecStart=/usr/bin/dotnet $run_path/$dll.dll --urls=http://localhost:$local_port/
    WorkingDirectory=$run_path
    Restart=on-failure
    RestartPreventExitStatus=10

    [Install]
    WantedBy=multi-user.target" > /etc/systemd/system/$service_name.service
    systemctl enable $service_name.service
    systemctl start $service_name.service
}

add_source()
{
    # dotnet
    wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -r -s)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb && rm ./packages-microsoft-prod.deb
    # caddy
    cat /etc/apt/sources.list.d/caddy-fury.list | grep -q caddy || echo "deb [trusted=yes] https://apt.fury.io/caddy/ /" | tee -a /etc/apt/sources.list.d/caddy-fury.list
    # sql server
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
    add-apt-repository "$(curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -r -s)/mssql-server-2019.list)"
    # node js
    curl -sL https://deb.nodesource.com/setup_14.x | sudo -E bash -
}

update_connection()
{
    dbString="$1"
    path="$2"
    dbFixedString=$(echo '    "DatabaseConnection": "'$dbString'",')
    dbLineNumber=$(grep -n DatabaseConnection $path/appsettings.Production.json | cut -d : -f 1)
    pattern=$(echo $dbLineNumber)s/.*/$dbFixedString/
    sed -i "$pattern" $path/appsettings.Production.json
}

update_domain()
{
    domainString="$1"
    path="$2"
    domainFixedString=$(echo '      "Server": "'$domainString'",')
    domainLineNumber=$(grep -n '"server.kahla.app"' $path/appsettings.Production.json | cut -d : -f 1)
    pattern=$(echo $domainLineNumber)s/.*/$domainFixedString/
    sed -i "$pattern" $path/appsettings.Production.json
}

update_keys()
{
    publicKey="$1"
    privateKey="$2"
    path="$3"
    publicKeyFixedString=$(echo '      "PublicKey": "'$publicKey'",')
    privateKeyFixedString=$(echo '      "PrivateKey": "'$privateKey'",')
    publicKeyNumber=$(grep -n '"PublicKey"' $path/appsettings.Production.json | cut -d : -f 1)
    privateKeyNumber=$(grep -n '"PrivateKey"' $path/appsettings.Production.json | cut -d : -f 1)
    pattern1=$(echo $publicKeyNumber)s/.*/$publicKeyFixedString/
    pattern2=$(echo $privateKeyNumber)s/.*/$privateKeyFixedString/
    sed -i "$pattern1" $path/appsettings.Production.json
    sed -i "$pattern2" $path/appsettings.Production.json
}

install_kahla()
{
    server="$1"
    appId="$2"
    appSecret="$3"
    echo "Installing Kahla to domain $server..."

    # Valid domain is required
    ip=$(dig +short $server)
    if [[ "$server" == "" ]] || [[ "$ip" == "" ]]; then
        echo "You must specify your valid server domain. Try execute with 'bash -s www.a.com'"
        return 9
    fi

    if [[ $(ifconfig) == *"$ip"* ]]; 
    then
        echo "The ip result from domian $server is: $ip and it is your current machine IP!"
    else
        echo "The ip result from domian $server is: $ip and it seems not to be your current machine IP!"
        return 9
    fi

    # Valid app is required
    archonResponse=$(curl https://archon.aiursoft.com/API/AccessToken?appId=$appId\&appSecret=$appSecret)
    if [[ $archonResponse == *":0"* ]]; 
    then
        echo "AppId and AppSecret for Aiursoft Developer Center is correct!"
    else
        echo "AppId and AppSecret for Aiursoft Developer Center is not valid! Please register an valid app at https://developer.aiursoft.com"
        return 9
    fi

    port=$(get_port)
    dbPassword=$(uuidgen)
    echo "Using internal port: $port"

    cd ~

    # Enable BBR
    enable_bbr

    # Set production mode
    set_production

    # Install basic packages
    echo "Installing packages..."
    _=$(add_source)
    _=$(apt install -y apt-transport-https curl git vim dotnet-sdk-3.1 caddy mssql-server nodejs)

    # Init database password
    MSSQL_SA_PASSWORD=$dbPassword MSSQL_PID='express' /opt/mssql/bin/mssql-conf -n setup accept-eula
    systemctl restart mssql-server

    # Download the source code
    ls | grep -q Kahla && rm ./Kahla -rf
    git clone -b master https://github.com/AiursoftWeb/Kahla.git

    # Build the code
    echo 'Building the source code...'
    kahla_path="$(pwd)/apps/kahlaApp"
    dotnet publish -c Release -o $kahla_path ./Kahla/Kahla.Server/Kahla.Server.csproj
    rm ~/Kahla -rf
    cat $kahla_path/appsettings.json > $kahla_path/appsettings.Production.json

    # Configure appsettings.json
    connectionString="Server=tcp:127.0.0.1,1433;Initial Catalog=Kahla;Persist Security Info=False;User ID=sa;Password=$dbPassword;MultipleActiveResultSets=True;Connection Timeout=30;"
    npm install web-push -g
    web-push generate-vapid-keys > ./temp.txt
    publicKey=$(cat ./temp.txt | sed -n 5p)
    privateKey=$(cat ./temp.txt | sed -n 8p)
    rm ./temp.txt
    update_connection "$connectionString" $kahla_path
    update_domain "$server" $kahla_path
    update_keys $publicKey $privateKey $kahla_path

    # Register kahla service
    echo "Registering Kahla service..."
    register_service "kahla" $port $kahla_path "Kahla.Server"

    # Config caddy
    echo 'Configuring the web proxy...'
    add_caddy_proxy $server $port

    # Config firewall
    open_port 443
    open_port 80
    enable_firewall

    # Finish the installation
    echo "Successfully installed Kahla as a service in your machine! Please open https://$server to try it now!"
    echo "Successfully installed mssql as a service in your machine! The port is not opened so you can't connect!"
    echo "Successfully installed caddy as a service in your machine!"
    echo "You can connect to your server from a Kahla.App. Open https://www.kahla.app"
    echo "You can open your database via: sudo ufw allow 1433/tcp"
    echo "You can access your database via: $server:1433 with username: sa and password: $dbPassword"
    echo "Your database data file is located at: /var/opt/mssql/"
    echo "Your web data file is located at: $kahla_path"
    echo "Your web server config file is located at: /etc/caddy/Caddyfile"
    echo "Strongly suggest run 'sudo apt upgrade' and reboot when convience!"
}

install_kahla "$@"
