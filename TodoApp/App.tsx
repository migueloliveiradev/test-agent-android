import { StatusBar } from 'expo-status-bar';
import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  FlatList,
  SafeAreaView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';

type AuthResponse = { token: string };
type TodoTask = {
  id: number;
  title: string;
  content: string;
  date: string;
  status: string;
};

const getToday = () =>
  new Date(Date.now() - new Date().getTimezoneOffset() * 60_000).toISOString().slice(0, 10);
const defaultApiUrl = process.env.EXPO_PUBLIC_API_URL ?? 'http://localhost:5184';

export default function App() {
  const [apiUrl, setApiUrl] = useState(defaultApiUrl);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [token, setToken] = useState('');

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [date, setDate] = useState(getToday());
  const [status, setStatus] = useState('Pendente');
  const [editingId, setEditingId] = useState<number | null>(null);

  const [tasks, setTasks] = useState<TodoTask[]>([]);

  const headers = useMemo(
    () => ({
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }),
    [token]
  );

  const auth = async (endpoint: 'register' | 'login') => {
    try {
      const response = await fetch(`${apiUrl}/api/auth/${endpoint}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        Alert.alert('Erro', `${endpoint === 'register' ? 'Registro' : 'Login'} falhou`);
        return;
      }

      const data: AuthResponse = await response.json();
      setToken(data.token);
    } catch {
      Alert.alert('Erro', 'Não foi possível conectar à API.');
    }
  };

  const loadTasks = useCallback(async () => {
    try {
      const response = await fetch(`${apiUrl}/api/tasks`, { headers });
      if (!response.ok) {
        Alert.alert('Erro', 'Não foi possível carregar as tarefas.');
        return;
      }

      const data: TodoTask[] = await response.json();
      setTasks(data);
    } catch {
      Alert.alert('Erro', 'Não foi possível carregar as tarefas.');
    }
  }, [apiUrl, headers]);

  useEffect(() => {
    if (token) {
      loadTasks();
    }
  }, [token, loadTasks]);

  const saveTask = async () => {
    const payload = {
      title,
      content,
      date: `${date}T00:00:00Z`,
      status,
    };

    const url = editingId ? `${apiUrl}/api/tasks/${editingId}` : `${apiUrl}/api/tasks`;
    const method = editingId ? 'PUT' : 'POST';

    try {
      const response = await fetch(url, {
        method,
        headers,
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        Alert.alert('Erro', 'Não foi possível salvar a tarefa.');
        return;
      }

      setTitle('');
      setContent('');
      setDate(getToday());
      setStatus('Pendente');
      setEditingId(null);
      await loadTasks();
    } catch {
      Alert.alert('Erro', 'Não foi possível salvar a tarefa.');
    }
  };

  const removeTask = async (id: number) => {
    try {
      const response = await fetch(`${apiUrl}/api/tasks/${id}`, {
        method: 'DELETE',
        headers,
      });

      if (response.ok) {
        await loadTasks();
      }
    } catch {
      Alert.alert('Erro', 'Não foi possível excluir a tarefa.');
    }
  };

  const startEdit = (task: TodoTask) => {
    setEditingId(task.id);
    setTitle(task.title);
    setContent(task.content);
    setDate(task.date.slice(0, 10));
    setStatus(task.status);
  };

  return (
    <SafeAreaView style={styles.container}>
      <Text style={styles.title}>CRUD de Tarefas</Text>
      <TextInput style={styles.input} value={apiUrl} onChangeText={setApiUrl} placeholder="URL da API" />

      {!token ? (
        <View>
          <TextInput style={styles.input} value={email} onChangeText={setEmail} placeholder="Email" autoCapitalize="none" />
          <TextInput style={styles.input} value={password} onChangeText={setPassword} placeholder="Senha" secureTextEntry />
          <View style={styles.row}>
            <Button title="Registrar" onPress={() => auth('register')} />
            <Button title="Login" onPress={() => auth('login')} />
          </View>
        </View>
      ) : (
        <View style={styles.main}>
          <TextInput style={styles.input} value={title} onChangeText={setTitle} placeholder="Título" />
          <TextInput style={styles.input} value={content} onChangeText={setContent} placeholder="Conteúdo" />
          <TextInput style={styles.input} value={date} onChangeText={setDate} placeholder="Data (YYYY-MM-DD)" />
          <TextInput style={styles.input} value={status} onChangeText={setStatus} placeholder="Status" />
          <View style={styles.row}>
            <Button title={editingId ? 'Atualizar' : 'Criar'} onPress={saveTask} />
            <Button title="Atualizar Lista" onPress={loadTasks} />
          </View>

          <FlatList
            data={tasks}
            keyExtractor={(item) => item.id.toString()}
            renderItem={({ item }) => (
              <View style={styles.taskCard}>
                <Text style={styles.taskTitle}>{item.title}</Text>
                <Text>{item.content}</Text>
                <Text>{new Date(item.date).toLocaleDateString()}</Text>
                <Text>Status: {item.status}</Text>
                <View style={styles.row}>
                  <Button title="Editar" onPress={() => startEdit(item)} />
                  <Button title="Excluir" onPress={() => removeTask(item.id)} />
                </View>
              </View>
            )}
          />
        </View>
      )}
      <StatusBar style="auto" />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 16,
    backgroundColor: '#fff',
  },
  main: {
    flex: 1,
    gap: 8,
  },
  title: {
    fontSize: 24,
    fontWeight: '700',
    marginBottom: 12,
  },
  input: {
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 8,
    padding: 10,
    marginBottom: 8,
  },
  row: {
    flexDirection: 'row',
    gap: 8,
    marginBottom: 8,
  },
  taskCard: {
    borderWidth: 1,
    borderColor: '#eee',
    borderRadius: 8,
    padding: 10,
    marginBottom: 8,
    gap: 4,
  },
  taskTitle: {
    fontWeight: '700',
  },
});
