#!/bin/bash

# DormSystem Docker Images Build Script
# Скрипт для збірки всіх Docker образів

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${GREEN}================================${NC}"
echo -e "${GREEN}DormSystem Docker Images Builder${NC}"
echo -e "${GREEN}================================${NC}"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo -e "${RED}Docker is not installed. Please install Docker first.${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Docker is installed${NC}"
echo ""

# Ask for image tag
read -p "Enter image tag (default: latest): " IMAGE_TAG
IMAGE_TAG=${IMAGE_TAG:-latest}

echo -e "${BLUE}Building images with tag: ${IMAGE_TAG}${NC}"
echo ""

# Function to build an image
build_image() {
    local service_name=$1
    local dockerfile_path=$2
    local context_path=$3
    local image_name="dormsystem/${service_name}:${IMAGE_TAG}"

    echo -e "${YELLOW}Building ${service_name}...${NC}"
    if docker build -f "$dockerfile_path" -t "$image_name" "$context_path"; then
        echo -e "${GREEN}✓ ${service_name} built successfully${NC}"
    else
        echo -e "${RED}✗ Failed to build ${service_name}${NC}"
        return 1
    fi
    echo ""
}

# Build backend services
echo -e "${BLUE}=== Building Backend Services ===${NC}"
build_image "auth-service" "Services/Auth/Auth.API/Dockerfile" "."
build_image "events-service" "Services/Events/Events.API/Dockerfile" "."
build_image "rooms-service" "Services/Rooms/Rooms.API/Dockerfile" "."
build_image "booking-service" "Services/Booking/Booking.API/Dockerfile" "."
build_image "inspections-service" "Services/Inspections/Inspections.API/Dockerfile" "."
build_image "notification-service" "Services/NotificationCore/NotificationCore.API/Dockerfile" "."
build_image "telegram-service" "Services/TelegramAgent/TelegramAgent.API/Dockerfile" "."
build_image "file-service" "Services/FileStorage/FileStorage.API/Dockerfile" "."

# Build API Gateway
echo -e "${BLUE}=== Building Infrastructure ===${NC}"
build_image "api-gateway" "Infrastructure-services/ApiGateways/ApiGateway.YARP/Dockerfile" "."

# Build Frontend
echo -e "${BLUE}=== Building Frontend ===${NC}"
build_image "frontend" "frontend/dorm-app/Dockerfile" "frontend/dorm-app"

echo -e "${GREEN}================================${NC}"
echo -e "${GREEN}All images built successfully!${NC}"
echo -e "${GREEN}================================${NC}"
echo ""

# Show built images
echo -e "${YELLOW}Built images:${NC}"
docker images | grep "dormsystem" | grep "$IMAGE_TAG"

echo ""
echo -e "${BLUE}Next steps:${NC}"
echo "1. Deploy to Kubernetes: cd k8s && ./deploy.sh"
echo "2. Or push to registry: docker push dormsystem/<service-name>:${IMAGE_TAG}"
