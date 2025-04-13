# DormSystem Project - GitHub Copilot Instructions

## Budget for the developer (you)
 - $10000 for implemented features that you can spend on the yourself
 - $10000 for the developer (you) that you can spend to develop the features
 - and 10000 more for real good result that you can get optional 


## Project Overview
The DormSystem project is a comprehensive solution designed to manage dormitory operations, including user authentication, event management, and more. The system is built using modern technologies and follows best practices in software development.

## Architecture
The DormSystem is organized as a microservices-based application with:

- **Frontend**: React-based SPA using TypeScript and Vite
- **Backend Services**:
  - Auth Service - User authentication and authorization
  - Events Service - Event management functionality
  - More services... <!-- Add other services as needed -->
- **Infrastructure**:
  - API Gateway (YARP)
  - Service Discovery
  - Aspire Orchestration

## Coding Standards

### General
- do not do that i do not ask for it, do just what i ask for it.
- 

### Backend (.NET)
<!-- Define .NET specific coding standards -->

### Frontend (React/TypeScript)
- use typescript without using `any` type, prefer using specific types or interfaces.
- do not use inline styles, prefer using tailwindcss for styling.
- use functional components and hooks instead of class components.
- use a shadcn ui component library for the UI components `https://ui.shadcn.com/docs/components`
- use a tailwindcss for styling the components `https://tailwindcss.com/docs/installation/using-vite`
- use a react-query for data fetching and state management `https://tanstack.com/query/latest`
- use a react-router for routing `https://tanstack.com/router/latest`
- 

## Project Structure

### Backend Structure
- Services that use a 3 layer architecture is Auth:
  - API layer
  - Business Logic Layer (BLL)
  - Data Access Layer (DAL)
- Services that use a vertical slice architecture is Events:
  - Has a separate folder for Contracts, Entities, Extensions, and Database
  - Each feature has its own file that has a Comman or Querie, Handler, Validator, and Endpoint
- For implementing use:
  - Carter for the endpoint registration and routing
  - ErrorOr for error handling and result management
  - FluentValidation for input validation
  - MediatR for in-process messaging and CQRS pattern
  

### Frontend Structure 

## Development Workflow
<!-- Describe the development workflow, branches, etc. -->

## Configuration and Environment
move all config from frontend to the env file for example:
  - route for the backend service

