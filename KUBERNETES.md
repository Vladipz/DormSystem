# Інтеграція Kubernetes для DormSystem

## 📋 Зміст

1. [Що таке Kubernetes і навіщо він потрібен](#що-таке-kubernetes-і-навіщо-він-потрібен)
2. [Архітектура доступу в Kubernetes](#архітектура-доступу-в-kubernetes)
3. [Структура проекту](#структура-проекту)
4. [Передумови](#передумови)
5. [Швидкий старт](#швидкий-старт)
6. [Детальна інструкція з розгортання](#детальна-інструкція-з-розгортання)
7. [Налаштування та конфігурація](#налаштування-та-конфігурація)
8. [Моніторинг та управління](#моніторинг-та-управління)
9. [Усунення несправностей](#усунення-несправностей)
10. [Production налаштування](#production-налаштування)

---

## Що таке Kubernetes і навіщо він потрібен

### 🤔 Що таке Kubernetes?

**Kubernetes (K8s)** — це система оркестрації контейнерів з відкритим вихідним кодом, яка автоматизує розгортання, масштабування та управління контейнерними додатками.

### ✅ Чому Kubernetes потрібен для DormSystem?

**DormSystem** — це мікросервісний додаток з 8+ сервісами. Kubernetes надає:

1. **Автоматичне відновлення** (Self-healing)
   - Якщо сервіс падає, Kubernetes автоматично його перезапускає
   - Заміна несправних контейнерів без простою

2. **Горизонтальне масштабування**
   - Автоматичне збільшення кількості інстансів при високому навантаженні
   - Зменшення інстансів при низькому навантаженні

3. **Балансування навантаження**
   - Розподіл трафіку між кількома інстансами сервісу
   - Забезпечення високої доступності

4. **Керування конфігурацією**
   - Централізоване зберігання налаштувань (ConfigMaps)
   - Безпечне зберігання секретів (Secrets)

5. **Оновлення без простою** (Rolling Updates)
   - Поступова заміна старих версій новими
   - Можливість відкату до попередньої версії

6. **Ізоляція ресурсів**
   - Кожен сервіс має власні обмеження по CPU та пам'яті
   - Запобігання впливу одного сервісу на інші

---

## Архітектура доступу в Kubernetes

### 📊 Схема роботи

```
┌─────────────────────────────────────────────────────────────┐
│                         ІНТЕРНЕТ                             │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
            ┌────────────────────────┐
            │   Ingress Controller   │ ◄── Точка входу (HTTP/HTTPS)
            │      (Nginx)           │     Один IP для всіх сервісів
            └────────────┬───────────┘
                         │
         ┌───────────────┴────────────────┐
         │                                │
         ▼                                ▼
┌─────────────────┐            ┌──────────────────┐
│  Frontend       │            │  API Gateway     │
│  Service        │            │  Service         │
│  (ClusterIP)    │            │  (ClusterIP)     │
└─────────────────┘            └────────┬─────────┘
                                        │
                    ┌───────────────────┼────────────────┐
                    │                   │                │
                    ▼                   ▼                ▼
            ┌────────────┐      ┌────────────┐  ┌────────────┐
            │   Auth     │      │  Events    │  │   Rooms    │
            │  Service   │      │  Service   │  │  Service   │
            │ (ClusterIP)│      │(ClusterIP) │  │(ClusterIP) │
            └─────┬──────┘      └─────┬──────┘  └─────┬──────┘
                  │                   │                │
                  ▼                   ▼                ▼
         ┌────────────────┐  ┌────────────────┐ ┌──────────┐
         │   PostgreSQL   │  │   PostgreSQL   │ │  SQLite  │
         │  (StatefulSet) │  │  (StatefulSet) │ │  (PVC)   │
         └────────────────┘  └────────────────┘ └──────────┘
                                        │
                                        ▼
                               ┌────────────────┐
                               │    RabbitMQ    │
                               │  (StatefulSet) │
                               └────────────────┘
```

### 🔑 Типи сервісів у Kubernetes

#### 1. **ClusterIP** (внутрішній доступ)
```yaml
type: ClusterIP
```
- Сервіс доступний **тільки всередині кластера**
- Використовується для: мікросервіси, бази даних
- Приклад: `auth-service`, `events-service`
- Доступ: `http://auth-service:8080` (тільки з інших Pod'ів)

#### 2. **NodePort** (зовнішній доступ через порт на ноді)
```yaml
type: NodePort
port: 80
nodePort: 30080
```
- Відкриває порт на **всіх нодах кластера**
- Використовується для: розробки, тестування
- Приклад доступу: `http://NODE_IP:30080`
- Діапазон портів: 30000-32767

#### 3. **LoadBalancer** (хмарний балансувальник)
```yaml
type: LoadBalancer
```
- Створює **зовнішній IP** через хмарного провайдера
- Використовується для: production (GCP, AWS, Azure)
- Автоматично отримує публічний IP
- Приклад: API Gateway

#### 4. **Ingress** (HTTP/HTTPS маршрутизація)
```yaml
kind: Ingress
```
- **Один IP для всіх сервісів**
- Підтримка HTTPS/SSL
- Маршрутизація по доменах та шляхах
- Найкращий варіант для production

### 🌐 Як працює доступ до додатку

#### Сценарій 1: Користувач заходить на сайт

```
1. Користувач → https://dorm-system.com
2. DNS → IP адреса Ingress Controller
3. Ingress → Frontend Service (ClusterIP)
4. Frontend Service → Frontend Pod (Nginx)
5. Відповідь повертається користувачу
```

#### Сценарій 2: Фронтенд робить API запит

```
1. Frontend → https://dorm-system.com/api/auth/login
2. Ingress → API Gateway Service
3. API Gateway → Auth Service (ClusterIP)
4. Auth Service → PostgreSQL
5. Відповідь повертається через ланцюг
```

#### Сценарій 3: Внутрішня комунікація між сервісами

```
1. Events Service потребує інформацію про користувача
2. Events Service → http://auth-service:8080/users/123
3. Kubernetes DNS розв'язує auth-service → IP Pod'а
4. Запит йде напряму до Auth Service Pod'а
5. Відповідь повертається
```

### 🔒 Service Discovery (Виявлення сервісів)

Kubernetes автоматично створює DNS записи для сервісів:

```bash
# Повна форма
<service-name>.<namespace>.svc.cluster.local

# Приклади
auth-service.dormsystem.svc.cluster.local
postgres-service.dormsystem.svc.cluster.local

# Коротка форма (в межах того ж namespace)
auth-service
postgres-service
```

---

## Структура проекту

```
DormSystem/
├── Services/                           # Backend мікросервіси
│   ├── Auth/Auth.API/Dockerfile       # Docker образ Auth сервісу
│   ├── Events/Events.API/Dockerfile   # Docker образ Events сервісу
│   ├── Rooms/Rooms.API/Dockerfile     # Docker образ Rooms сервісу
│   ├── Booking/Booking.API/Dockerfile
│   ├── Inspections/Inspections.API/Dockerfile
│   ├── NotificationCore/NotificationCore.API/Dockerfile
│   ├── TelegramAgent/TelegramAgent.API/Dockerfile
│   └── FileStorage/FileStorage.API/Dockerfile
│
├── Infrastructure-services/
│   └── ApiGateways/ApiGateway.YARP/Dockerfile  # API Gateway
│
├── frontend/
│   └── dorm-app/
│       ├── Dockerfile                  # Docker образ Frontend
│       ├── nginx.conf                  # Nginx конфігурація
│       └── .dockerignore
│
├── k8s/                                # Kubernetes manifests
│   ├── namespace.yaml                  # Namespace dormsystem
│   ├── configmap.yaml                  # Конфігурація
│   ├── secrets.yaml                    # Секрети (паролі, токени)
│   │
│   ├── postgres-statefulset.yaml       # PostgreSQL база даних
│   ├── rabbitmq-statefulset.yaml       # RabbitMQ черга повідомлень
│   │
│   ├── auth-service.yaml               # Auth мікросервіс
│   ├── events-service.yaml             # Events мікросервіс
│   ├── rooms-service.yaml              # Rooms мікросервіс
│   ├── booking-service.yaml
│   ├── inspections-service.yaml
│   ├── notification-service.yaml
│   ├── telegram-service.yaml
│   ├── file-service.yaml
│   │
│   ├── api-gateway.yaml                # API Gateway
│   ├── frontend.yaml                   # Frontend додаток
│   │
│   ├── ingress.yaml                    # Ingress контролер
│   ├── kustomization.yaml              # Kustomize конфігурація
│   └── deploy.sh                       # Скрипт для розгортання
│
├── helm/                               # Helm charts
│   └── dormsystem/
│       ├── Chart.yaml                  # Метадані Helm chart
│       ├── values.yaml                 # Значення за замовчуванням
│       └── .helmignore
│
└── KUBERNETES.md                       # Ця документація
```

---

## Передумови

### 🛠️ Необхідне програмне забезпечення

1. **Kubernetes кластер** (один з варіантів):
   - **Локальна розробка**: [Minikube](https://minikube.sigs.k8s.io/), [Kind](https://kind.sigs.k8s.io/), [Docker Desktop](https://www.docker.com/products/docker-desktop/)
   - **Хмарні провайдери**: Google GKE, Amazon EKS, Azure AKS
   - **Власний сервер**: kubeadm, k3s

2. **kubectl** — CLI для роботи з Kubernetes
   ```bash
   # Перевірка встановлення
   kubectl version --client
   ```

3. **Docker** — для збирання образів
   ```bash
   # Перевірка встановлення
   docker --version
   ```

4. **(Опціонально) Helm** — пакетний менеджер для Kubernetes
   ```bash
   # Перевірка встановлення
   helm version
   ```

### 📦 Встановлення Minikube (для локальної розробки)

#### Linux:
```bash
curl -LO https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64
sudo install minikube-linux-amd64 /usr/local/bin/minikube
```

#### macOS:
```bash
brew install minikube
```

#### Windows:
```powershell
choco install minikube
```

### 🚀 Запуск Minikube

```bash
# Запустити кластер
minikube start --cpus=4 --memory=8192

# Перевірити статус
minikube status

# Увімкнути Ingress addon
minikube addons enable ingress

# Отримати IP адресу
minikube ip
```

---

## Швидкий старт

### 🎯 За 5 хвилин до запущеного додатку

```bash
# 1. Перейти до директорії k8s
cd DormSystem/k8s

# 2. Оновити секрети в secrets.yaml (ОБОВ'ЯЗКОВО!)
nano secrets.yaml  # або vim, code

# 3. Запустити скрипт розгортання
./deploy.sh

# 4. Вибрати режим розгортання
# Введіть: 1 (для розробки)

# 5. Дочекатися завершення
# Скрипт автоматично розгорне всі компоненти

# 6. Отримати IP адресу
kubectl get ingress -n dormsystem

# 7. Відкрити додаток у браузері
# http://<INGRESS_IP>
```

### 🔍 Перевірка стану

```bash
# Переглянути всі Pod'и
kubectl get pods -n dormsystem

# Переглянути сервіси
kubectl get services -n dormsystem

# Переглянути логи конкретного сервісу
kubectl logs -f <pod-name> -n dormsystem
```

---

## Детальна інструкція з розгортання

### Крок 1: Підготовка Docker образів

#### Варіант A: Локальна збірка (для Minikube)

```bash
# Налаштувати Docker для використання Minikube
eval $(minikube docker-env)

# Зібрати всі образи
cd DormSystem

# Backend сервіси
docker build -f Services/Auth/Auth.API/Dockerfile -t dormsystem/auth-service:latest .
docker build -f Services/Events/Events.API/Dockerfile -t dormsystem/events-service:latest .
docker build -f Services/Rooms/Rooms.API/Dockerfile -t dormsystem/rooms-service:latest .
docker build -f Services/Booking/Booking.API/Dockerfile -t dormsystem/booking-service:latest .
docker build -f Services/Inspections/Inspections.API/Dockerfile -t dormsystem/inspections-service:latest .
docker build -f Services/NotificationCore/NotificationCore.API/Dockerfile -t dormsystem/notification-service:latest .
docker build -f Services/TelegramAgent/TelegramAgent.API/Dockerfile -t dormsystem/telegram-service:latest .
docker build -f Services/FileStorage/FileStorage.API/Dockerfile -t dormsystem/file-service:latest .

# API Gateway
docker build -f Infrastructure-services/ApiGateways/ApiGateway.YARP/Dockerfile -t dormsystem/api-gateway:latest .

# Frontend
docker build -f frontend/dorm-app/Dockerfile -t dormsystem/frontend:latest frontend/dorm-app/

# Перевірити створені образи
docker images | grep dormsystem
```

#### Варіант B: Push до Docker Hub (для production)

```bash
# Авторизуватися в Docker Hub
docker login

# Тегувати образи з вашим username
docker tag dormsystem/auth-service:latest YOUR_DOCKERHUB_USERNAME/auth-service:latest

# Завантажити образи
docker push YOUR_DOCKERHUB_USERNAME/auth-service:latest

# Повторити для всіх сервісів
```

### Крок 2: Налаштування секретів

```bash
cd k8s

# Відредагувати secrets.yaml
nano secrets.yaml
```

**Важливі секрети для заміни:**

```yaml
# PostgreSQL (використовувати сильний пароль)
POSTGRES_PASSWORD: "ВАШ_ПАРОЛЬ_POSTGRES"

# RabbitMQ (використовувати сильний пароль)
RABBITMQ_DEFAULT_PASS: "ВАШ_ПАРОЛЬ_RABBITMQ"

# JWT Secret (згенерувати випадковий рядок)
JWT_SECRET: "супер-секретний-ключ-мінімум-32-символи"

# Telegram Bot Token (отримати від @BotFather)
TELEGRAM_BOT_TOKEN: "1234567890:ABCdefGHIjklMNOpqrsTUVwxyz"
```

**Генерація безпечного JWT secret:**

```bash
# Linux/macOS
openssl rand -base64 32

# або
head -c 32 /dev/urandom | base64
```

### Крок 3: Налаштування ConfigMap

```bash
nano configmap.yaml
```

**Важливі налаштування:**

```yaml
# CORS origins (оновити для production)
CORS_ORIGINS: "https://ваш-домен.com"
```

### Крок 4: Розгортання компонентів

#### Варіант A: Використання deploy.sh (рекомендовано)

```bash
./deploy.sh
```

#### Варіант B: Ручне розгортання

```bash
# 1. Створити namespace
kubectl apply -f namespace.yaml

# 2. Застосувати конфігурацію
kubectl apply -f configmap.yaml
kubectl apply -f secrets.yaml

# 3. Розгорнути інфраструктуру
kubectl apply -f postgres-statefulset.yaml
kubectl apply -f rabbitmq-statefulset.yaml

# Дочекатися готовності баз даних (2-3 хвилини)
kubectl wait --for=condition=ready pod -l app=postgres -n dormsystem --timeout=300s
kubectl wait --for=condition=ready pod -l app=rabbitmq -n dormsystem --timeout=300s

# 4. Розгорнути мікросервіси
kubectl apply -f auth-service.yaml
kubectl apply -f events-service.yaml
kubectl apply -f rooms-service.yaml
kubectl apply -f booking-service.yaml
kubectl apply -f inspections-service.yaml
kubectl apply -f notification-service.yaml
kubectl apply -f telegram-service.yaml
kubectl apply -f file-service.yaml

# 5. Розгорнути API Gateway та Frontend
kubectl apply -f api-gateway.yaml
kubectl apply -f frontend.yaml

# 6. Розгорнути Ingress
kubectl apply -f ingress.yaml
```

### Крок 5: Перевірка розгортання

```bash
# Переглянути статус всіх Pod'ів
kubectl get pods -n dormsystem

# Очікуваний вихід:
# NAME                                  READY   STATUS    RESTARTS   AGE
# auth-service-xxx-yyy                  1/1     Running   0          2m
# events-service-xxx-yyy                1/1     Running   0          2m
# ...

# Переглянути сервіси
kubectl get svc -n dormsystem

# Переглянути Ingress
kubectl get ingress -n dormsystem

# Детальна інформація
kubectl describe ingress dormsystem-ingress -n dormsystem
```

### Крок 6: Доступ до додатку

#### Для Minikube:

```bash
# Отримати IP
minikube ip

# Відкрити в браузері
# http://<MINIKUBE_IP>

# АБО використати minikube tunnel (в окремому терміналі)
minikube tunnel
# Потім доступ через: http://localhost
```

#### Для хмарних провайдерів:

```bash
# Дочекатися отримання зовнішнього IP
kubectl get ingress -n dormsystem -w

# Використати External IP для доступу
```

---

## Налаштування та конфігурація

### 🔧 Масштабування сервісів

```bash
# Збільшити кількість реплік Auth сервісу
kubectl scale deployment auth-service --replicas=5 -n dormsystem

# Автоматичне масштабування (HPA)
kubectl autoscale deployment auth-service \
  --cpu-percent=70 \
  --min=2 \
  --max=10 \
  -n dormsystem
```

### 🔄 Оновлення сервісів

```bash
# Оновити образ
kubectl set image deployment/auth-service \
  auth-service=dormsystem/auth-service:v2.0.0 \
  -n dormsystem

# Відкатити оновлення
kubectl rollout undo deployment/auth-service -n dormsystem

# Переглянути історію оновлень
kubectl rollout history deployment/auth-service -n dormsystem
```

### 🗄️ Backup баз даних

```bash
# PostgreSQL backup
kubectl exec -it postgres-0 -n dormsystem -- \
  pg_dump -U dormsystem auth_db > auth_db_backup.sql

# Відновлення
kubectl exec -i postgres-0 -n dormsystem -- \
  psql -U dormsystem auth_db < auth_db_backup.sql
```

### 📊 Ресурси та ліміти

Налаштування в YAML файлах:

```yaml
resources:
  requests:         # Мінімальні ресурси
    memory: "128Mi"
    cpu: "100m"
  limits:           # Максимальні ресурси
    memory: "256Mi"
    cpu: "200m"
```

---

## Моніторинг та управління

### 📈 Корисні команди kubectl

```bash
# Переглянути логи
kubectl logs -f <pod-name> -n dormsystem

# Переглянути логи всіх контейнерів з певним label
kubectl logs -l app=auth-service -n dormsystem --all-containers=true

# Виконати команду в Pod'і
kubectl exec -it <pod-name> -n dormsystem -- /bin/bash

# Переслати порт з Pod'а на локальну машину
kubectl port-forward <pod-name> 8080:8080 -n dormsystem

# Переглянути події
kubectl get events -n dormsystem --sort-by='.lastTimestamp'

# Переглянути використання ресурсів
kubectl top pods -n dormsystem
kubectl top nodes
```

### 🎯 Дебагінг

```bash
# Детальна інформація про Pod
kubectl describe pod <pod-name> -n dormsystem

# Перевірити стан Deployment
kubectl describe deployment auth-service -n dormsystem

# Переглянути конфігурацію
kubectl get configmap dormsystem-config -n dormsystem -o yaml

# Переглянути секрети (base64 encoded)
kubectl get secret dormsystem-secrets -n dormsystem -o yaml
```

### 🌐 Доступ до сервісів

```bash
# Доступ до RabbitMQ Management UI
kubectl port-forward svc/rabbitmq-service 15672:15672 -n dormsystem
# Відкрити: http://localhost:15672

# Доступ до PostgreSQL
kubectl port-forward svc/postgres-service 5432:5432 -n dormsystem
# Підключитися: psql -h localhost -U dormsystem -d auth_db

# Доступ до API Gateway напряму
kubectl port-forward svc/api-gateway 8080:80 -n dormsystem
# Відкрити: http://localhost:8080
```

---

## Усунення несправностей

### ❌ Pod не запускається

```bash
# Перевірити статус
kubectl get pods -n dormsystem

# Детальна інформація
kubectl describe pod <pod-name> -n dormsystem

# Переглянути логи
kubectl logs <pod-name> -n dormsystem

# Переглянути попередні логи (якщо Pod перезапускався)
kubectl logs <pod-name> -n dormsystem --previous
```

**Типові проблеми:**

1. **ImagePullBackOff** — не може завантажити образ
   ```bash
   # Перевірити наявність образу
   docker images | grep dormsystem

   # Для Minikube переконатися, що використовується правильний Docker
   eval $(minikube docker-env)
   ```

2. **CrashLoopBackOff** — контейнер падає при запуску
   ```bash
   # Переглянути логи
   kubectl logs <pod-name> -n dormsystem

   # Перевірити змінні оточення
   kubectl describe pod <pod-name> -n dormsystem | grep -A 10 Environment
   ```

3. **Pending** — Pod очікує на ресурси
   ```bash
   # Перевірити події
   kubectl describe pod <pod-name> -n dormsystem

   # Перевірити наявність вільних ресурсів
   kubectl top nodes
   ```

### 🔌 Проблеми з підключенням до БД

```bash
# Перевірити, чи PostgreSQL запущена
kubectl get pods -l app=postgres -n dormsystem

# Перевірити логи PostgreSQL
kubectl logs postgres-0 -n dormsystem

# Тестове підключення
kubectl exec -it postgres-0 -n dormsystem -- \
  psql -U dormsystem -d auth_db -c "SELECT 1;"
```

### 🚪 Ingress не працює

```bash
# Перевірити Ingress
kubectl get ingress -n dormsystem

# Детальна інформація
kubectl describe ingress dormsystem-ingress -n dormsystem

# Перевірити Ingress Controller
kubectl get pods -n ingress-nginx

# Для Minikube перевірити addon
minikube addons list | grep ingress
```

---

## Production налаштування

### 🔒 Безпека

#### 1. Використовувати Kubernetes Secrets правильно

```bash
# Створити секрет з файлу
kubectl create secret generic jwt-secret \
  --from-literal=JWT_SECRET=$(openssl rand -base64 32) \
  -n dormsystem

# Створити секрет для Docker registry
kubectl create secret docker-registry regcred \
  --docker-server=docker.io \
  --docker-username=YOUR_USERNAME \
  --docker-password=YOUR_PASSWORD \
  -n dormsystem
```

#### 2. Налаштувати RBAC (Role-Based Access Control)

```yaml
# service-account.yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: dormsystem-sa
  namespace: dormsystem
---
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: dormsystem-role
  namespace: dormsystem
rules:
  - apiGroups: [""]
    resources: ["pods", "services"]
    verbs: ["get", "list"]
```

#### 3. Network Policies

```yaml
# network-policy.yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: auth-service-policy
  namespace: dormsystem
spec:
  podSelector:
    matchLabels:
      app: auth-service
  policyTypes:
    - Ingress
  ingress:
    - from:
        - podSelector:
            matchLabels:
              app: api-gateway
      ports:
        - protocol: TCP
          port: 8080
```

### 📜 HTTPS/SSL

#### Варіант 1: Cert-Manager + Let's Encrypt

```bash
# Встановити cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Створити ClusterIssuer
cat <<EOF | kubectl apply -f -
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: your-email@example.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
      - http01:
          ingress:
            class: nginx
EOF

# Оновити Ingress для використання SSL
# (розкоментувати TLS секцію в ingress.yaml)
```

#### Варіант 2: Власний сертифікат

```bash
# Створити Secret з сертифікатом
kubectl create secret tls dormsystem-tls-secret \
  --cert=path/to/cert.crt \
  --key=path/to/cert.key \
  -n dormsystem
```

### 🗄️ Persistent Storage

#### Для хмарних провайдерів (GCP, AWS, Azure)

```yaml
# storage-class.yaml
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata:
  name: fast-ssd
provisioner: kubernetes.io/gce-pd  # GCP
# provisioner: kubernetes.io/aws-ebs  # AWS
# provisioner: kubernetes.io/azure-disk  # Azure
parameters:
  type: pd-ssd
  replication-type: regional-pd
```

### 📊 Моніторинг (Prometheus + Grafana)

```bash
# Встановити Prometheus Operator
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm install prometheus prometheus-community/kube-prometheus-stack \
  -n monitoring --create-namespace

# Доступ до Grafana
kubectl port-forward svc/prometheus-grafana 3000:80 -n monitoring
# Відкрити: http://localhost:3000
# Логін: admin / prom-operator
```

### 📝 Централізоване логування (EFK Stack)

```bash
# Встановити Elasticsearch + Fluentd + Kibana
helm repo add elastic https://helm.elastic.co
helm install elasticsearch elastic/elasticsearch -n logging --create-namespace
helm install kibana elastic/kibana -n logging
helm install fluentd stable/fluentd-elasticsearch -n logging
```

---

## Додаткові ресурси

### 📚 Документація

- [Kubernetes офіційна документація](https://kubernetes.io/docs/)
- [kubectl шпаргалка](https://kubernetes.io/docs/reference/kubectl/cheatsheet/)
- [Helm документація](https://helm.sh/docs/)
- [.NET на Kubernetes](https://learn.microsoft.com/en-us/dotnet/architecture/containerized-lifecycle/)

### 🎓 Навчальні матеріали

- [Kubernetes Tutorial for Beginners](https://www.youtube.com/watch?v=X48VuDVv0do)
- [Deploying .NET Core Apps to Kubernetes](https://www.youtube.com/watch?v=gf84j4dPijQ)

---

## Підтримка

Якщо у вас виникли питання або проблеми з розгортанням, створіть issue у репозиторії проекту.

---

**Створено для DormSystem** | Версія 1.0.0 | 2025
