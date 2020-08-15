aiur() { arg="$( cut -d ' ' -f 2- <<< "$@" )" && curl -sL https://github.com/AiursoftWeb/AiurScript/raw/master/$1.sh | sudo bash -s $arg; }
kahla_path="/opt/apps/KahlaServer"

update_domain()
{
    domainString="$1"
    path="$2"
    domainFixedString=$(echo '      "Server": "'$domainString'",')
    domainLineNumber=$(grep -n '"server.kahla.app"' $path/appsettings.Production.json | cut -d : -f 1)
    pattern=$(echo $domainLineNumber)s/.*/$domainFixedString/
    sed -i "$pattern" $path/appsettings.Production.json
}

install_kahla()
{
    server="$1"

    if [[ $(curl -sL ifconfig.me) == "$(dig +short $1)" ]]; 
    then
        echo "IP is correct."
    else
        echo "$1 is not your current machine IP!"
        return 9
    fi

    if [[ $(curl -s https://archon.aiursoft.com/API/AccessToken?appId=$2\&appSecret=$3) == *":0"* ]]; 
    then
    else
        echo "AppId and AppSecret for Aiursoft Developer Center is not valid! Please register an valid app at https://developer.aiursoft.com"
        return 9
    fi

    port=$(aiur network/get_port) && echo "Using internal port: $port"
    dbPassword=$(uuidgen)
    aiur network/enable_bbr
    aiur system/set_aspnet_prod
    aiur install/caddy
    aiur install/dotnet
    aiur install/node
    aiur mssql/config_password $dbPassword
    aiur git/clone_to AiursoftWeb/Kahla ./Kahla
    dotnet publish -c Release -o $kahla_path ./Kahla/Kahla.Server/Kahla.Server.csproj && rm ./Kahla -rf
    cat $kahla_path/appsettings.json > $kahla_path/appsettings.Production.json
    update_domain "$1" $kahla_path
    connectionString="Server=tcp:127.0.0.1,1433;Initial Catalog=Kahla;Persist Security Info=False;User ID=sa;Password=$dbPassword;MultipleActiveResultSets=True;Connection Timeout=30;"
    aiur text/edit_json "DatabaseConnection" "$connectionString" $kahla_path
    npm install web-push -g
    web-push generate-vapid-keys > ./temp.txt
    publicKey=$(cat ./temp.txt | sed -n 5p)
    privateKey=$(cat ./temp.txt | sed -n 8p)
    rm ./temp.txt
    aiur text/edit_json "PublicKey" "$publicKey" $kahla_path
    aiur text/edit_json "PrivateKey" "$privateKey" $kahla_path
    aiur text/edit_json "KahlaAppSecret" "$appSecret" $kahla_path
    aiur text/edit_json "KahlaAppId" "$appId" $kahla_path
    aiur text/edit_json "UserIconsSiteName" "$(uuidgen)" $kahla_path
    aiur text/edit_json "UserFilesSiteName" "$(uuidgen)" $kahla_path
    aiur services/register_aspnet_service "kahla" $port $kahla_path "Kahla.Server"
    aiur caddy/add_proxy $1 $port
    aiur firewall/enable_firewall
    aiur firewall/open_port 443
    aiur firewall/open_port 80

    # Finish the installation
    echo "Successfully installed Kahla as a service in your machine! Please open https://$1 to try it now!"
    echo "The port 1433 is not opened. You can open your database to public via: sudo ufw allow 1433/tcp"
    echo "You can connect to your server from a Kahla.App. Download the client at: https://www.kahla.app"
    echo "Database identity: $1:1433 with username: sa and password: $dbPassword"
    echo ""
    echo "Your database data file is located at: /var/opt/mssql/. Please back up them regularly."
    echo "Your web data file is located at: $kahla_path"
}

install_kahla "$@"
