#!/usr/bin/env bash
set -euo pipefail

export DEBIAN_FRONTEND=noninteractive

apt-get update
apt-get install -y ca-certificates curl gnupg lsb-release git

install -m 0755 -d /etc/apt/keyrings

if [ ! -f /etc/apt/keyrings/docker.asc ]; then
  curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
  chmod a+r /etc/apt/keyrings/docker.asc
fi

source /etc/os-release
arch="$(dpkg --print-architecture)"
repo="deb [arch=${arch} signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu ${VERSION_CODENAME} stable"

if [ ! -f /etc/apt/sources.list.d/docker.list ] || ! grep -Fq "${repo}" /etc/apt/sources.list.d/docker.list; then
  echo "${repo}" > /etc/apt/sources.list.d/docker.list
fi

apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

systemctl enable docker
systemctl start docker
usermod -aG docker vagrant

mkdir -p /home/vagrant/.local/bin

cat >/home/vagrant/.local/bin/dormsystem-up <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
cd /workspace
docker compose -f docker-compose.vm.yml up -d
EOF

cat >/home/vagrant/.local/bin/dormsystem-down <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
cd /workspace
docker compose -f docker-compose.vm.yml down
EOF

cat >/home/vagrant/.local/bin/dormsystem-ps <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
cd /workspace
docker compose -f docker-compose.vm.yml ps
EOF

chmod +x /home/vagrant/.local/bin/dormsystem-up /home/vagrant/.local/bin/dormsystem-down /home/vagrant/.local/bin/dormsystem-ps
chown -R vagrant:vagrant /home/vagrant/.local
