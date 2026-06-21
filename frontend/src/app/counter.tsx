"use client";

import { Button } from "@/components/ui/button";
import { useEffect, useState } from "react";
import { JSX } from "react/jsx-runtime";

export default function Counter(): JSX.Element {
  const [counter, setCounter] = useState(0);
  useEffect(() => {
    console.log("update render");
  }, [counter]);

  const handleClick = () => {
    setCounter(counter + 1);
  };

  const isWin = counter >= 10;

  return (
    <div className="flex flex-col gap-4">
      <CoolCount count={counter} />
      <Button onClick={handleClick}>Увеличить</Button>
      {isWin && <span>You WIN!</span>}
    </div>
  );
}

type Props = {
  count: number;
};

function CoolCount({ count }: Props) {
  return <span className="text-rose-300">{count}</span>;
}
