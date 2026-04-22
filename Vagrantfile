Vagrant.configure("2") do |config|
  config.vm.box = "ubuntu/jammy64"
  config.vm.hostname = "dormsystem-stress"

  config.vm.network "private_network", ip: "192.168.56.20"
  config.vm.network "forwarded_port", guest: 5095, host: 5095
  config.vm.network "forwarded_port", guest: 3000, host: 3000
  config.vm.network "forwarded_port", guest: 9090, host: 9090
  config.vm.network "forwarded_port", guest: 15672, host: 15672

  config.vm.provider "virtualbox" do |vb|
    vb.name = "dormsystem-stress"
    vb.cpus = 4
    vb.memory = 4096
  end

  config.vm.synced_folder ".", "/workspace", type: "virtualbox"

  config.vm.provision "shell", path: "vagrant/bootstrap.sh"
end
