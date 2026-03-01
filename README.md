# test-agent-android

Projeto com:
- API ASP.NET Core 10 (`/TodoApi`) com registro/login e CRUD de tarefas
- App React Native com Expo 55.0.4 e React Native 0.84.1 (`/TodoApp`)

## Campos de tarefa
- título
- conteúdo
- data
- status

## API
Execute:

```bash
cd /home/runner/work/test-agent-android/test-agent-android/TodoApi
dotnet run --urls http://localhost:5281
```

Endpoints:
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET/POST/PUT/DELETE /api/tasks`

## App Expo
Execute:

```bash
cd /home/runner/work/test-agent-android/test-agent-android/TodoApp
npm install
npm run start
```
