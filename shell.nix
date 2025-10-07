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
    # Podman Compose only (no Docker)
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
    echo "🐳 Using Podman as container runtime"
    echo ""
    echo "📊 Available commands:"
    echo "  dotnet build      - Build the solution"
    echo "  dotnet run        - Run the WebAPI or CLI"
    echo "  dotnet ef         - Entity Framework commands"
    echo "  podman-compose up - Start PostgreSQL and PgAdmin"
    echo ""
    echo "🌐 WebAPI will be available at: https://localhost:7032"
    echo "🗄️  PgAdmin will be available at: http://localhost:8080"
    echo "   Email: wss@wss.com | Password: WSS"
    echo ""

    # Set up .NET tools path
    export PATH="$HOME/.dotnet/tools:$PATH"
    export LD_LIBRARY_PATH="${pkgs.ncurses}/lib:$LD_LIBRARY_PATH"

    # Function to check if Podman is running
    check_podman() {
      if ! command -v podman >/dev/null 2>&1; then
        echo "❌ Podman is not installed"
        return 1
      fi
      if ! podman info >/dev/null 2>&1; then
        echo "⚠️  Podman is not running or not configured properly."
        return 1
      fi
      return 0
    }

    # Function to start database setup
    setup_database() {
      echo "🐳 Starting containers with Podman Compose..."
      podman-compose up -d

      echo "📊 Waiting for PostgreSQL to be ready..."
      sleep 3

      echo "🔄 Running database migrations..."
      cd Server
      dotnet ef database update
      cd ..

      echo "✅ Database setup completed!"
      exit 0
    }

    # Function to start client application
    start_client() {
      echo "🚀 Starting Client application..."
      cd Client
      dotnet run
      exit 0
    }

    # Check if Podman is available
    if ! check_podman; then
      echo "❌ Please ensure Podman is installed and running, then try again."
      exit 1
    fi

    # Present options to user
    echo "Please choose an option:"
    echo "1) Setup database only (start containers + run migrations)"
    echo "2) Start client application"
    echo ""
    echo "Enter 1 or 2:"

    read -r choice
    case "$choice" in
      1)
        setup_database
        ;;
      2)
        start_client
        ;;
      *)
        echo "❌ Invalid choice. Please enter 1 or 2."
        echo "You can also run these commands manually:"
        echo "  Option 1: podman-compose up -d && cd Server && dotnet ef database update"
        echo "  Option 2: cd Client && dotnet run"
        ;;
    esac
  '';
}
  
