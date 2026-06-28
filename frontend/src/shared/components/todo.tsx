"use client";

import { useState } from "react";
import { Card, CardHeader, CardTitle } from "@/src/shared/components/ui/card";
import { Badge } from "@/src/shared/components/ui/badge";
import { Checkbox } from "@/src/shared/components/ui/checkbox";
import { Input } from "@/src/shared/components/ui/input";
import { useTodos } from "@/src/shared/hooks/use-todos";

export default function Todo() {
  const { todos, addTodo, removeTodo, toggleTodo, completedCount } = useTodos();
  const [input, setInput] = useState<string>("");

  const handleAdd = () => {
    addTodo(input);
    setInput("");
  };

  return (
    <div className="flex flex-col w-full max-w-2xl gap-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Мои задачи</h1>
          <p className="text-sm text-muted-foreground mt-1">
            {completedCount} из {todos.length} выполнено
          </p>
        </div>
      </div>

      {/* Add todo */}
      <div className="flex gap-2">
        <Input
          placeholder="новая задача"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleAdd()}
        />
        <button
          onClick={handleAdd}
          className="shrink-0 rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground transition-all hover:bg-primary/80"
        >
          Добавить
        </button>
      </div>

      {/* Todo list */}
      <div className="grid gap-3">
        {todos.map((todo) => (
          <Card
            key={todo.id}
            size="sm"
            className="transition-all hover:border-primary/30"
          >
            <CardHeader>
              <div className="flex items-center gap-3">
                <Checkbox
                  checked={todo.completed}
                  id={`todo-${todo.id}`}
                  onCheckedChange={() => toggleTodo(todo.id)}
                />
                <div className="flex-1 min-w-0">
                  <CardTitle
                    className={`text-sm ${
                      todo.completed ? "text-muted-foreground line-through" : ""
                    }`}
                  >
                    {todo.title}
                  </CardTitle>
                </div>
                <Badge
                  variant={todo.completed ? "default" : "secondary"}
                  className="shrink-0"
                >
                  {todo.completed ? "Готово" : "В процессе"}
                </Badge>
                <button
                  onClick={() => removeTodo(todo.id)}
                  className="shrink-0 flex items-center justify-center h-7 w-7 rounded-md text-muted-foreground/50 transition-all hover:bg-destructive/10 hover:text-destructive"
                  title="Удалить"
                >
                  <svg
                    className="h-4 w-4"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    strokeWidth={2}
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                    />
                  </svg>
                </button>
              </div>
            </CardHeader>
          </Card>
        ))}
      </div>
    </div>
  );
}
