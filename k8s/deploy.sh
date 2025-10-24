#!/bin/bash

# DormSystem Kubernetes Deployment Script
# This script helps deploy the DormSystem application to Kubernetes

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}================================${NC}"
echo -e "${GREEN}DormSystem Kubernetes Deployment${NC}"
echo -e "${GREEN}================================${NC}"
echo ""

# Check if kubectl is installed
if ! command -v kubectl &> /dev/null; then
    echo -e "${RED}kubectl is not installed. Please install kubectl first.${NC}"
    exit 1
fi

# Check if kubectl can connect to cluster
if ! kubectl cluster-info &> /dev/null; then
    echo -e "${RED}Cannot connect to Kubernetes cluster. Please check your kubeconfig.${NC}"
    exit 1
fi

echo -e "${GREEN}✓ kubectl is installed and connected to cluster${NC}"
echo ""

# Ask for deployment mode
echo "Select deployment mode:"
echo "1) Development (with dev ingress)"
echo "2) Production (with domain-based ingress)"
read -p "Enter choice [1-2]: " DEPLOY_MODE

# Apply namespace
echo -e "${YELLOW}Creating namespace...${NC}"
kubectl apply -f namespace.yaml

# Apply ConfigMaps and Secrets
echo -e "${YELLOW}Applying ConfigMaps...${NC}"
kubectl apply -f configmap.yaml

echo -e "${YELLOW}Applying Secrets...${NC}"
read -p "Have you updated the secrets in secrets.yaml? (yes/no): " SECRETS_UPDATED
if [ "$SECRETS_UPDATED" != "yes" ]; then
    echo -e "${RED}Please update the secrets in secrets.yaml before deploying!${NC}"
    exit 1
fi
kubectl apply -f secrets.yaml

# Deploy infrastructure (PostgreSQL and RabbitMQ)
echo -e "${YELLOW}Deploying PostgreSQL...${NC}"
kubectl apply -f postgres-statefulset.yaml

echo -e "${YELLOW}Deploying RabbitMQ...${NC}"
kubectl apply -f rabbitmq-statefulset.yaml

# Wait for databases to be ready
echo -e "${YELLOW}Waiting for PostgreSQL to be ready...${NC}"
kubectl wait --for=condition=ready pod -l app=postgres -n dormsystem --timeout=300s

echo -e "${YELLOW}Waiting for RabbitMQ to be ready...${NC}"
kubectl wait --for=condition=ready pod -l app=rabbitmq -n dormsystem --timeout=300s

# Deploy microservices
echo -e "${YELLOW}Deploying microservices...${NC}"
kubectl apply -f auth-service.yaml
kubectl apply -f events-service.yaml
kubectl apply -f rooms-service.yaml
kubectl apply -f booking-service.yaml
kubectl apply -f inspections-service.yaml
kubectl apply -f notification-service.yaml
kubectl apply -f telegram-service.yaml
kubectl apply -f file-service.yaml

# Deploy API Gateway
echo -e "${YELLOW}Deploying API Gateway...${NC}"
kubectl apply -f api-gateway.yaml

# Deploy Frontend
echo -e "${YELLOW}Deploying Frontend...${NC}"
kubectl apply -f frontend.yaml

# Deploy Ingress
echo -e "${YELLOW}Deploying Ingress...${NC}"
if [ "$DEPLOY_MODE" = "1" ]; then
    echo "Deploying development ingress..."
    kubectl apply -f ingress.yaml
else
    echo "Deploying production ingress..."
    read -p "Enter your domain name (e.g., dorm-system.example.com): " DOMAIN
    sed "s/dorm-system.example.com/$DOMAIN/g" ingress.yaml | kubectl apply -f -
fi

echo ""
echo -e "${GREEN}================================${NC}"
echo -e "${GREEN}Deployment Complete!${NC}"
echo -e "${GREEN}================================${NC}"
echo ""

# Show deployment status
echo -e "${YELLOW}Checking deployment status...${NC}"
kubectl get pods -n dormsystem

echo ""
echo -e "${YELLOW}Services:${NC}"
kubectl get services -n dormsystem

echo ""
echo -e "${YELLOW}Ingress:${NC}"
kubectl get ingress -n dormsystem

echo ""
echo -e "${GREEN}To access the application:${NC}"
if [ "$DEPLOY_MODE" = "1" ]; then
    echo "- Get the Ingress IP: kubectl get ingress -n dormsystem"
    echo "- Access via: http://<INGRESS_IP>"
else
    echo "- Ensure DNS is configured to point to your Ingress IP"
    echo "- Access via: https://$DOMAIN"
fi

echo ""
echo -e "${GREEN}Useful commands:${NC}"
echo "- View logs: kubectl logs -f <pod-name> -n dormsystem"
echo "- Check pods: kubectl get pods -n dormsystem"
echo "- Describe pod: kubectl describe pod <pod-name> -n dormsystem"
echo "- Scale deployment: kubectl scale deployment <deployment-name> --replicas=3 -n dormsystem"
echo "- Delete all: kubectl delete namespace dormsystem"
