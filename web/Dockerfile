# Build stage (discarded: only the compiled files reach the final image).
FROM node:22-alpine AS build
WORKDIR /app

# 1) Copy only the package files: the install layer stays cached until dependencies change.
COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm,sharing=locked \
    npm ci --no-audit --no-fund

# 2) Now copy the source code and build (uses environment.prod.ts: apiBaseUrl "/api").
COPY . .
RUN npm run build -- --configuration production


# Final image: nginx serves the static files and forwards /api to the API container.
FROM nginx:1.27-alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/web/browser /usr/share/nginx/html
EXPOSE 80
