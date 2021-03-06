sudo echo -n
RED='\033[0;31m'
NC='\033[0m'
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/ubuntu xenial main" | sudo tee /etc/apt/sources.list.d/mono-official.list
sudo apt-get update
sudo apt-get install mono-devel
echo "Mono installed."
sudo cp -a ./PushNotificationServer/. /usr/local/PushNotificationServer/
echo "Push Notification Server files successfully moved."
sudo cp pushnotificationserver.service /etc/systemd/system/pushnotificationserver.service
echo "Push Notification Service moved to Service dir"
sudo systemctl daemon-reload
sudo systemctl start pushnotificationserver.service
sudo systemctl status pushnotificationserver.service
echo "${RED}Push Notification Server Started on http://+:3010. To add new notifications, add them to /usr/local/PushNotificationServer/Notifications/ and wait a few minutes${NC}"