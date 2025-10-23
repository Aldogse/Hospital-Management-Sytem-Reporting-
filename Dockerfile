FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

# Copy project files
COPY . .

#cd to Report_and_Analytics_API
RUN cd Report_and_Analytics_API

# Build the application
RUN dotnet build 

EXPOSE 80

# Run the application
CMD ["dotnet", "run", "--urls", "http://0.0.0.0:80", "--project", "./Report_and_Analytics_API"]