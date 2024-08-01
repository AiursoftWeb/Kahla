aiur() { arg="$( cut -d ' ' -f 2- <<< "$@" )" && curl -sL https://gitlab.aiursoft.cn/aiursoft/aiurscript/-/raw/master/$1.sh | sudo bash -s $arg; }

kahla_path="/opt/apps/KahlaServer"
dbPassword=$(uuidgen)
port=$(aiur network/get_port)
connectionString="Server=tcp:127.0.0.1,1433;Database=Kahla;uid=sa;Password=$dbPassword;MultipleActiveResultSets=True;"

install_kahla()
{
    aiur network/enable_bbr
    aiur install/caddy
    aiur install/dotnet
    aiur install/node
    aiur install/jq
    aiur install/sql_server
    aiur mssql/config_password $dbPassword
    aiur git/clone_to https://gitlab.aiursoft.cn/aiursoft/Kahla ./Kahla
    aiur dotnet/publish $kahla_path ./Kahla/Kahla.Server/Kahla.Server.csproj
    cat $kahla_path/appsettings.json > $kahla_path/appsettings.Production.json

    npm install web-push -g --loglevel verbose
    web-push generate-vapid-keys > ./temp.txt
    publicKey=$(cat ./temp.txt | sed -n 5p)
    privateKey=$(cat ./temp.txt | sed -n 8p)
    rm ./temp.txt

    aiur text/edit_json "VapidKeys.PublicKey" "$publicKey" $kahla_path/appsettings.Production.json
    aiur text/edit_json "VapidKeys.PrivateKey" "$privateKey" $kahla_path/appsettings.Production.json
    aiur text/edit_json "ConnectionStrings.DatabaseConnection" "$connectionString" $kahla_path/appsettings.Production.json
    aiur text/edit_json "AppDomain[2].Server" "$1" $kahla_path/appsettings.Production.json
    aiur text/edit_json "KahlaAppId" "$2" $kahla_path/appsettings.Production.json
    aiur text/edit_json "KahlaAppSecret" "$3" $kahla_path/appsettings.Production.json
    aiur text/edit_json "UserIconsSiteName" "$(uuidgen)" $kahla_path/appsettings.Production.json
    aiur text/edit_json "UserFilesSiteName" "$(uuidgen)" $kahla_path/appsettings.Production.json
    aiur services/register_aspnet_service "kahla" $port $kahla_path "Kahla.Server"
    aiur caddy/add_proxy $1 $port

    # Finish the installation
    echo "Successfully installed Kahla as a service in your machine! Please open https://$1 to try it now!"
    echo "The port 1433 is not opened. You can open your database to public via: sudo ufw allow 1433/tcp"
    echo "You can connect to your server from a Kahla.App. Download the client at: https://www.kahla.app"
    echo "Database identity: $1:1433 with username: sa and password: $dbPassword"
    echo ""
    echo "Your database data file is located at: /var/opt/mssql/. Please back up them regularly."
    echo "Your web data file is located at: $kahla_path"
    rm ./Kahla -rf
}

install_kahla "$@"
