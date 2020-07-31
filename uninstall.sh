
delete_service()
{
    service="$1" 
    systemctl stop $service
    systemctl disable $service
    rm /etc/systemd/system/$service
    rm /etc/systemd/system/$service # and symlinks that might be related
    rm /usr/lib/systemd/system/$service 
    rm /usr/lib/systemd/system/$service # and symlinks that might be related
    systemctl daemon-reload
    systemctl reset-failed
}

delete_service "caddy.service"
delete_service "kahla.service"

rm ~/apps -rvf
rm ~/Kahla -rvf
rm /etc/caddy -rvf

apt remove caddy -y

echo "Successfully uninstalled tracer on your machine!"