FROM node:20-alpine AS build
WORKDIR /app

RUN corepack enable

COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile

COPY . .

ARG VITE_API_GATEWAY_URL=http://localhost:5095
ARG VITE_NOTIFICATIONS_DELIVERY_MODE=websocket
ENV VITE_API_GATEWAY_URL=$VITE_API_GATEWAY_URL
ENV VITE_NOTIFICATIONS_DELIVERY_MODE=$VITE_NOTIFICATIONS_DELIVERY_MODE

RUN yarn build

FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
