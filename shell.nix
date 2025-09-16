{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  name = "wss-dev-shell";

  # Required packages
  buildInputs = [
    # .NET 8 SDK
    pkgs.dotnet-sdk_9

    pkgs.ncurses

    # Entity Framework Core tools
    pkgs.dotnet-ef

    # Both Docker Compose and Podman Compose
    pkgs.docker-compose
    pkgs.podman-compose

    # Git for version control
    pkgs.git
  ];

  # Environment variables
  DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "false";
  ASPNETCORE_ENVIRONMENT = "Development";

  # Database connection environment variables (matches your appsettings.json)
  DATABASE_HOST = "127.0.0.1";
  DATABASE_PORT = "5432";
  DATABASE_NAME = "wss_dev";
  DATABASE_USER = "wss";
  DATABASE_PASS = "WSS";

  # WebAPI configuration
  WEBAPI_SCHEME = "https";
  WEBAPI_DOMAIN = "localhost";
  WEBAPI_PORT = "7032";

  # Shell hook - runs when you enter the nix-shell
  shellHook = ''
    echo "🚀 Welcome to WSS (Why So Serious?) Development Environment"
    echo "📦 Loaded dependencies: .NET 8, ICU, PostgreSQL, EF Core tools"
    echo ""
    echo "📊 Available commands:"
    echo "  dotnet build      - Build the solution"
    echo "  dotnet run        - Run the WebAPI or CLI"
    echo "  dotnet ef         - Entity Framework commands"
    echo "  docker-compose up - Start PostgreSQL and PgAdmin (Docker)"
    echo "  podman-compose up - Start PostgreSQL and PgAdmin (Podman)"
    echo ""
    echo "🌐 WebAPI will be available at: https://localhost:7032"
    echo "🗄️  PgAdmin will be available at: http://localhost:8080"
    echo "   Email: wss@wss.com | Password: WSS"
    echo ""

    # Set up .NET tools path
    export PATH="$HOME/.dotnet/tools:$PATH"
    export LD_LIBRARY_PATH="${pkgs.ncurses}/lib:$LD_LIBRARY_PATH"

    # Detect container runtime (Docker or Podman)
    detect_container_runtime() {
      if command -v docker >/dev/null 2>&1 && docker info >/dev/null 2>&1; then
        echo "docker"
      elif command -v podman >/dev/null 2>&1 && podman info >/dev/null 2>&1; then
        echo "podman"
      else
        echo "none"
      fi
    }

    # Get the appropriate compose command
    get_compose_command() {
      local runtime="$1"
      case "$runtime" in
        "docker") echo "docker-compose" ;;
        "podman") echo "podman-compose" ;;
        *) echo "" ;;
      esac
    }

    # Function to check if container runtime is running
    check_container_runtime() {
      local runtime="$1"
      case "$runtime" in
        "docker")
          if ! docker info >/dev/null 2>&1; then
            echo "⚠️  Docker daemon is not running. Please start Docker to use containers."
            return 1
          fi
          ;;
        "podman")
          if ! podman info >/dev/null 2>&1; then
            echo "⚠️  Podman is not running or not configured properly."
            return 1
          fi
          ;;
        "none")
          echo "❌ No container runtime found. Please install Docker or Podman."
          return 1
          ;;
      esac
      return 0
    }

    # Function to wait for PostgreSQL to be ready
    wait_for_postgres() {
      local runtime="$1"
      local compose_cmd="$2"

      echo "⏳ Waiting for PostgreSQL to be ready..."
      local timeout=30
      local count=0

      # Get container name based on runtime
      local container_name="wss-postgres"
      if [ "$runtime" = "podman" ]; then
        container_name="wss-postgres-1"  # Podman Compose uses different naming
      fi

      while true; do
        # Try different methods to check PostgreSQL readiness
        if [ "$runtime" = "docker" ]; then
          if docker exec "$container_name" pg_isready -U wss -d wss_dev >/dev/null 2>&1; then
            break
          fi
        elif [ "$runtime" = "podman" ]; then
          if podman exec "$container_name" pg_isready -U wss -d wss_dev >/dev/null 2>&1; then
            break
          fi
        fi

        count=$((count + 1))
        if [ $count -ge $timeout ]; then
          echo "❌ PostgreSQL failed to start within $timeout seconds"
          return 1
        fi
        echo -n "."
        sleep 1
      done
      echo ""
      echo "✅ PostgreSQL is ready!"
      return 0
    }

    # Function to run database migrations
    run_migrations() {
      echo "🔄 Running database migrations..."
      if dotnet ef database update; then
        echo "✅ Database migrations completed successfully"
        return 0
      else
        echo "❌ Database migrations failed"
        return 1
      fi
    }

    # Function to start the application
    start_application() {
      echo "🚀 Starting WebAPI with HTTPS profile..."
      dotnet run --launch-profile https
    }

    # Function to cleanup containers
    cleanup_containers() {
      local runtime="$1"
      local compose_cmd="$2"

      echo "🧹 Cleaning up containers..."
      $compose_cmd down
    }

    # Main startup sequence
    startup_sequence() {
      # Detect container runtime
      local runtime=$(detect_container_runtime)
      local compose_cmd=$(get_compose_command "$runtime")

      if [ -z "$compose_cmd" ]; then
        echo "❌ No container runtime found. Please install Docker or Podman."
        return 1
      fi

      # Check if container runtime is running
      if ! check_container_runtime "$runtime"; then
        return 1
      fi

      echo "🐳 Using $runtime with $compose_cmd"

      # Start containers in the background
      echo "📦 Starting containers..."
      $compose_cmd up -d

      # Wait for PostgreSQL to be ready
      if ! wait_for_postgres "$runtime" "$compose_cmd"; then
        echo "❌ Failed to start PostgreSQL"
        cleanup_containers "$runtime" "$compose_cmd"
        return 1
      fi

      # Run database migrations
      if ! run_migrations; then
        echo "❌ Failed to run migrations"
        cleanup_containers "$runtime" "$compose_cmd"
        return 1
      fi

      # Start the application
      start_application

      # Cleanup when application exits
      echo "🛑 Application stopped, cleaning up containers..."
      cleanup_containers "$runtime" "$compose_cmd"
    }

    # Ask user if they want to run the full startup sequence
    echo "Do you want to start the full application stack? (y/N)"
    read -r response
    case "$response" in
      [yY][eE][sS]|[yY])
        echo "Starting full application stack..."
        startup_sequence
        ;;
      *)
        echo "Manual mode:"
        echo "  Run 'docker-compose up' or 'podman-compose up' to start database"
        echo "  Run 'dotnet ef database update' for migrations"
        echo "  Run 'dotnet run --launch-profile https' to start API"
        ;;
    esac
  '';
}
