# Kubernetes для DormSystem - Швидкий старт

## 🚀 Швидке розгортання

### Крок 1: Підготовка

```bash
# Запустити Minikube (якщо використовуєте локально)
minikube start --cpus=4 --memory=8192
minikube addons enable ingress

# Налаштувати Docker для Minikube
eval $(minikube docker-env)
```

### Крок 2: Збірка Docker образів

```bash
cd DormSystem

# Зібрати всі образи
./build-images.sh  # АБО вручну:

docker build -f Services/Auth/Auth.API/Dockerfile -t dormsystem/auth-service:latest .
docker build -f Services/Events/Events.API/Dockerfile -t dormsystem/events-service:latest .
docker build -f Services/Rooms/Rooms.API/Dockerfile -t dormsystem/rooms-service:latest .
docker build -f Services/Booking/Booking.API/Dockerfile -t dormsystem/booking-service:latest .
docker build -f Services/Inspections/Inspections.API/Dockerfile -t dormsystem/inspections-service:latest .
docker build -f Services/NotificationCore/NotificationCore.API/Dockerfile -t dormsystem/notification-service:latest .
docker build -f Services/TelegramAgent/TelegramAgent.API/Dockerfile -t dormsystem/telegram-service:latest .
docker build -f Services/FileStorage/FileStorage.API/Dockerfile -t dormsystem/file-service:latest .
docker build -f Infrastructure-services/ApiGateways/ApiGateway.YARP/Dockerfile -t dormsystem/api-gateway:latest .
docker build -f frontend/dorm-app/Dockerfile -t dormsystem/frontend:latest frontend/dorm-app/
```

### Крок 3: Налаштування секретів

```bash
cd k8s

# Відредагувати secrets.yaml
nano secrets.yaml

# Оновити ці значення:
# - POSTGRES_PASSWORD
# - RABBITMQ_DEFAULT_PASS
# - JWT_SECRET (згенерувати: openssl rand -base64 32)
# - TELEGRAM_BOT_TOKEN (якщо використовується)
```

### Крок 4: Розгортання

```bash
# Використати автоматичний скрипт
./deploy.sh

# Вибрати режим 1 (Development)
```

### Крок 5: Доступ до додатку

```bash
# Отримати IP адресу
minikube ip
# АБО
kubectl get ingress -n dormsystem

# Відкрити в браузері
http://<MINIKUBE_IP>
```

---

## 📋 Корисні команди

```bash
# Переглянути всі Pod'и
kubectl get pods -n dormsystem

# Переглянути логи
kubectl logs -f <pod-name> -n dormsystem

# Переглянути сервіси
kubectl get svc -n dormsystem

# Видалити все
kubectl delete namespace dormsystem
```

---

## 📖 Детальна документація

Дивіться [KUBERNETES.md](../KUBERNETES.md) для повної документації.

---

## 🗂️ Структура файлів

- `namespace.yaml` - Kubernetes namespace
- `configmap.yaml` - Конфігурація додатку
- `secrets.yaml` - Секрети (паролі, токени)
- `postgres-statefulset.yaml` - PostgreSQL база даних
- `rabbitmq-statefulset.yaml` - RabbitMQ черга
- `*-service.yaml` - Мікросервіси
- `api-gateway.yaml` - API Gateway
- `frontend.yaml` - Frontend додаток
- `ingress.yaml` - Ingress контролер
- `deploy.sh` - Скрипт автоматичного розгортання

---

## ⚙️ Налаштування

### Масштабування

```bash
kubectl scale deployment auth-service --replicas=5 -n dormsystem
```

### Оновлення образу

```bash
kubectl set image deployment/auth-service \
  auth-service=dormsystem/auth-service:v2.0.0 \
  -n dormsystem
```

### Перезапуск сервісу

```bash
kubectl rollout restart deployment/auth-service -n dormsystem
```

---

## 🔧 Усунення несправностей

### Pod не запускається

```bash
kubectl describe pod <pod-name> -n dormsystem
kubectl logs <pod-name> -n dormsystem
```

### Перевірка підключення до БД

```bash
kubectl exec -it postgres-0 -n dormsystem -- \
  psql -U dormsystem -d auth_db -c "SELECT 1;"
```

### Доступ до RabbitMQ Management

```bash
kubectl port-forward svc/rabbitmq-service 15672:15672 -n dormsystem
# Відкрити: http://localhost:15672
```

---

## 📊 Архітектура

```
Internet → Ingress → API Gateway → Microservices → Databases
           ↓
        Frontend
```

---

Детальна інформація у [KUBERNETES.md](../KUBERNETES.md)
