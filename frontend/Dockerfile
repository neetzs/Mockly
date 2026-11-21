# ------------------------------------------------------------------------------
# Etapa 1: Build de la SPA con Node (Vite genera archivos estáticos en /app/dist)
# ------------------------------------------------------------------------------
FROM node:24-alpine AS build
WORKDIR /app

# Dependencias primero para aprovechar el cache de capas
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Vite "hornea" las variables VITE_* en el JS al compilar: la URL de la API se define acá.
# Es la URL que usa el NAVEGADOR del usuario, por eso apunta al puerto publicado en el host.
ARG VITE_API_BASE_URL=http://localhost:5067/api/v1
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# ------------------------------------------------------------------------------
# Etapa 2: Servir los estáticos con nginx (imagen final chica, sin Node)
# ------------------------------------------------------------------------------
FROM nginx:alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
