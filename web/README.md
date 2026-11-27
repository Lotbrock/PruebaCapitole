# Web (Angular)

SPA for the bookings API. Angular standalone components, signals, typed reactive forms, Bootstrap 5 (CSS only).

## Run

```bash
npm install
npm start        # http://localhost:4200
npm test         # unit tests (Vitest)
npm run build    # production build in dist/
```

The API must be running (`docker compose up -d`, port 8080). `ng serve` forwards `/api` to `http://localhost:8080` (see `proxy.conf.json`),
so no CORS setup is needed in development. If you run the API from Visual Studio instead (http://localhost:5229),
change the `target` in `proxy.conf.json`.

The API base URL is read from `src/environments/environment.ts` (development) and
`environment.prod.ts` (production build).

## Structure

```
src/app/
  core/        models, API service (the only place that uses HttpClient), error interceptor, utils
  shared/      GenericTable<T>, status badge, error alert (they know nothing about bookings)
  features/
    bookings/  booking-list, booking-details, booking-form
```
