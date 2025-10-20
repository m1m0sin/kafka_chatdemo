#!/usr/bin/env bash

check_status() {

  CONTAINER="connect"

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

restart_connect() {
  echo "Reiniciando contenedor connect.."
  podman restart connect
}

check_status

#Crear/Actualizar el JDBC Sink
curl -sS -X PUT http://localhost:8083/connectors/jdbc-sink-chat-messages/config \
  -H "Content-Type: application/json" \
  --data-binary @jdbc-sink-chat-messages.json

restart_connect

check_status

# Estado
curl -sS http://localhost:8083/connectors/jdbc-sink-chat-messages/status