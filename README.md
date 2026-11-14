# RaceDay-Part3
Part 3 of my race day application

## Run the MVC app in Docker

Build the image from the repository root:

```sh
docker build -t raceday-mvc .
```

Run it and open `http://localhost:8080`:

```sh
docker run --rm --name raceday-mvc \
  -p 8080:8080 \
  --add-host=host.docker.internal:host-gateway \
  -e ApiSettings__BaseUrl=http://host.docker.internal:5013/ \
  raceday-mvc
```

The API must be running and reachable on port `5013` on the host. On Docker
Desktop, `host.docker.internal` is available automatically; the `--add-host`
option handles it on Linux. If the API runs in another container, set
`ApiSettings__BaseUrl` to that API container's reachable service name and port.

The container serves HTTP on port `8080` for local use. Configure HTTPS before
exposing the app publicly.
