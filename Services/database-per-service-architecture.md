# DormSystem - Database Per Service Architecture

```mermaid
graph TB
    %% Services
    AS[Auth Service<br/>:5001]
    ES[Events Service<br/>:5002]
    RS[Rooms Service<br/>:5003]
    IS[Inspections Service<br/>:5004]
    BS[Booking Service<br/>TBD]
    NS[NotificationCore Service<br/>:5006]
    TS[TelegramAgent Service<br/>TBD]
    
    %% Databases
    AuthDB[(Auth Database<br/>PostgreSQL)]
    EventsDB[(Events Database<br/>PostgreSQL)]
    RoomsDB[(Rooms Database<br/>SQLite)]
    InspectionsDB[(Inspections Database<br/>SQLite)]
    NotificationDB[(Notification Database<br/>SQLite)]
    TelegramDB[(Telegram Database<br/>SQLite)]
    
    %% Message Bus
    MB[RabbitMQ<br/>Message Bus]
    
    %% Service to Database connections
    AS --- AuthDB
    ES --- EventsDB
    RS --- RoomsDB
    IS --- InspectionsDB
    NS --- NotificationDB
    TS --- TelegramDB
    
    %% Service to Message Bus connections
    AS -.-> MB
    ES -.-> MB
    RS -.-> MB
    IS -.-> MB
    BS -.-> MB
    NS -.-> MB
    TS -.-> MB
    
    %% Styling
    classDef serviceBox fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef dbPostgres fill:#336791,stroke:#fff,stroke-width:2px,color:#fff
    classDef dbSqlite fill:#003b57,stroke:#fff,stroke-width:2px,color:#fff
    classDef messageBus fill:#ff6f00,stroke:#fff,stroke-width:2px,color:#fff
    
    class AS,ES,RS,IS,BS,NS,TS serviceBox
    class AuthDB,EventsDB dbPostgres
    class RoomsDB,InspectionsDB,NotificationDB,TelegramDB dbSqlite
    class MB messageBus
```

## Architecture Overview

Each microservice in the DormSystem follows the **Database Per Service** pattern, ensuring:

- **Data Isolation**: Each service owns its data and schema
- **Technology Diversity**: Services can choose appropriate database technology
- **Independent Scaling**: Databases can be scaled independently
- **Service Autonomy**: No shared database dependencies

### Database Technologies Used

- **PostgreSQL**: Used by Auth and Events services for complex relational data
- **SQLite**: Used by Rooms, Inspections, NotificationCore, and TelegramAgent for simpler data requirements
- **RabbitMQ**: Enables asynchronous communication between services

### Service Ports
- Auth Service: 5001
- Events Service: 5002  
- Rooms Service: 5003
- Inspections Service: 5004
- Aspire Orchestration: 5005
- NotificationCore Service: 5006 