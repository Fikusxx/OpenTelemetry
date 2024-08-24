# How to use:


### 1. Build docker images manually, cause I suck at docker lol
- docker build . -t orders.api
- docker build . -t order.validator.service
- docker build . -t orders.view.service

### 2. Run docker compose
- docker-compose up -d

### 3. Wait for the apps to start up

### 4. Run request 
- go to <span style="color: yellow; font-weight:bold;"> Orders.http </span> and run the POST request
- in case of an error (which is fictional) - simply retry

### 5. Traces
- go to http://localhost:16686/search

### 6. Metrics
- go to http://localhost:3000/dashboards
- click <span style="color: yellow; font-weight:bold;"> "Create Dashboard" or "Import a dashboard" </span>
- custom metrics are <span style="color: yellow; font-weight:bold;"> "orders_created_total" && "orders_failed_total"</span>

### 7. Logs
- go to http://localhost:3000/connections/datasources/edit/loki
- click <span style="color: yellow; font-weight:bold;"> "Explore data" </span> at the top right corner
- run any query