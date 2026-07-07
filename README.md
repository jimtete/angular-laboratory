# Angular Learning Lab

A personal Angular playground used to learn Angular through small experiments, components, services, routing, forms, signals, HTTP requests, and mini-projects.

This repository is intentionally experimental. Some commits may contain incomplete or broken code while different Angular concepts are being tested.

## Project Environment

The project was initially created using:

```text
Angular CLI: 22.0.5
Node.js: 24.18.0
npm: 11.16.0
Operating System: Windows 64-bit
Stylesheet format: CSS
```

## Install Dependencies

Install the exact dependency versions stored in `package-lock.json`:

```powershell
npm ci
```

Use the following command instead when deliberately adding or updating dependencies:

```powershell
npm install
```

The `node_modules` directory is generated locally and should not be committed to GitHub.

## Run the Development Server

Start the Angular development server:

```powershell
npm start
```

Alternatively:

```powershell
ng serve
```

Open the application at:

```text
http://localhost:4200
```

To start the server and open the browser automatically:

```powershell
ng serve --open
```

Angular automatically rebuilds and reloads the application when source files are changed.

Stop the development server with:

```text
Ctrl + C
```

## Build the Project

Create a production build:

```powershell
npm run build
```

Alternatively:

```powershell
ng build
```

The generated build files are placed inside the `dist/` directory.

## Run Tests

Run the project's unit tests:

```powershell
npm test
```

Alternatively:

```powershell
ng test
```

## Watch Build

Build the project again whenever source files change:

```powershell
npm run watch
```

## Useful Angular CLI Commands

Generate a new component:

```powershell
ng generate component components/example
```

Short form:

```powershell
ng g c components/example
```

Generate a service:

```powershell
ng generate service services/example
```

Short form:

```powershell
ng g s services/example
```

Generate an interface:

```powershell
ng generate interface models/example
```

Generate a class:

```powershell
ng generate class models/example
```

Display the installed Angular versions:

```powershell
ng version
```

Display available Angular CLI commands:

```powershell
ng help
```

## Normal Git Workflow

Check changed files:

```powershell
git status
```

Stage all changes:

```powershell
git add .
```

Create a commit:

```powershell
git commit -m "describe the experiment"
```

Push the commit to GitHub:

```powershell
git push
```

Download changes made from another computer:

```powershell
git pull
```

A typical learning-session workflow is:

```powershell
git pull
npm ci
npm start
```

After finishing the session:

```powershell
git add .
git commit -m "experiment with Angular components"
git push
```

## Continuing on Another Computer

Install Git and Node.js, then run:

```powershell
git clone https://github.com/YOUR-USERNAME/angular-learning-lab.git
cd angular-learning-lab
npm ci
npm start
```

For an existing clone:

```powershell
cd angular-learning-lab
git pull
npm ci
npm start
```

## Project Structure

```text
angular-learning-lab/
├── public/                 Static public files
├── src/
│   ├── app/                Angular application code
│   ├── index.html          Main HTML document
│   ├── main.ts             Application entry point
│   └── styles.css          Global styles
├── angular.json            Angular workspace configuration
├── package.json            Project scripts and dependencies
├── package-lock.json       Exact dependency versions
├── tsconfig.json           TypeScript configuration
├── AGENTS.md               Instructions for compatible AI coding agents
└── README.md               Project documentation
```

## Important Files

### `package.json`

Contains the project dependencies and npm commands.

### `package-lock.json`

Stores the exact dependency versions required to reproduce the installation.

This file should be committed to Git.

### `angular.json`

Contains Angular workspace, build, development-server, and testing configuration.

### `src/app/`

Contains the main application components, services, models, routes, and related code.

### `src/styles.css`

Contains styles applied globally across the application.

### `AGENTS.md`

Contains project instructions that compatible AI coding agents, including Codex, can use while working in this repository.

## Security

Never commit sensitive information, including:

* passwords
* API keys
* access tokens
* private credentials
* production configuration
* `.env` files containing secrets

Before committing, always check:

```powershell
git status
```

## Learning Goals

This repository may include experiments involving:

* components and templates
* interpolation and property binding
* event binding
* signals and computed values
* conditional rendering
* loops and lists
* component inputs and outputs
* services and dependency injection
* routing
* template-driven forms
* reactive forms
* HTTP requests
* error handling
* local storage
* reusable UI components
* small Angular applications

## Notes

This is a learning repository rather than a production application. Code may be rewritten, deleted, intentionally broken, or replaced as Angular concepts are explored.
