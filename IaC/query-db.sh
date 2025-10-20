#!/usr/bin/env bash
check_status() {
  CONTAINER="postgres"

  STATUS=$(podman inspect --format '{{.State.Status}}' "$CONTAINER")

  if [ "$STATUS" != "running" ]; then
    echo "🚀 El contenedor '$CONTAINER' no está corriendo (estado actual: $STATUS). Iniciando..."
    podman start "$CONTAINER"
  else
    echo "✅ El contenedor '$CONTAINER' ya está en ejecución."
  fi

  HEALTHSTATUS=$(podman inspect --format='{{.State.Health.Status}}' "$CONTAINER")
  until [ "$HEALTHSTATUS" = "healthy" ]; do
    echo "⏳ Esperando que '$CONTAINER' esté listo..."
    sleep 5
    HEALTHSTATUS=$(podman inspect --format='{{.State.Health.Status}}' "$CONTAINER")
  done
  echo "✅ '$CONTAINER' está arriba."
}

check_status
podman exec -i postgres bash -lc "psql -U demo -d demo -h localhost" < query-db.sql