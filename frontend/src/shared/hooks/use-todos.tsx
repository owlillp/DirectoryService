"use client";

import { useState, useCallback } from "react";

export type Todo = {
  id: string;
  title: string;
  completed: boolean;
};

export function useTodos() {
  const [todos, setTodos] = useState<Todo[]>(() => []);

  const addTodo = useCallback((title: string) => {
    if (!title.trim()) return;
    setTodos((prev) => [
      ...prev,
      { id: crypto.randomUUID(), title: title.trim(), completed: false },
    ]);
  }, []);

  const removeTodo = useCallback((id: string) => {
    setTodos((prev) => prev.filter((todo) => todo.id !== id));
  }, []);

  const toggleTodo = useCallback((id: string) => {
    setTodos((prev) =>
      prev.map((todo) =>
        todo.id === id ? { ...todo, completed: !todo.completed } : todo,
      ),
    );
  }, []);

  const completedCount = todos.filter((t) => t.completed).length;

  return { todos, addTodo, removeTodo, toggleTodo, completedCount };
}
