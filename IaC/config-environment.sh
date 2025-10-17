#!/usr/bin/env bash

set -e  # Detiene la ejecución si un comando falla

setup_containers() {
    podman-compose up -d
}

install_jdbc_connector() {
    echo "Instalando el conector JDBC..."
    ./init-jdbc-connector.sh
}

init_orders() {
    echo "Inicializando la base de datos"
    ./init-orders.sh
}

init_ordersschema() {
    echo "Creando topicos y enviando mensajes a Kafka..."
    ./ordersschema.sh
}

config_jdbc_connector() {
    echo "Configurando el conector JDBC para consumir los mensajes de kafka y almancenarlos en la base de datos..."
    ./jdbc-sink-orders.sh
}

query_orders() {
    echo "Querying data..."
    ./query-orders.sh
}

main() {
    setup_containers
    sleep 5

    install_jdbc_connector
    sleep 5

    init_orders
    sleep 5

    init_ordersschema
    sleep 5

    config_jdbc_connector
    sleep 5

    query_orders
}

main
