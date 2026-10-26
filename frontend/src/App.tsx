import React, { useState } from "react";
import { Plus, Minus, RotateCcw, CheckCircle2 } from "lucide-react";

export default function App(): React.JSX.Element {
  const [count, setCount] = useState<number>(0);
  const [copied, setCopied] = useState<boolean>(false);

  const handleCopy = async (): Promise<void> => {
    await navigator.clipboard.writeText("git checkout develop");
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <main className="min-h-screen bg-background text-foreground flex items-center justify-center p-6 antialiased">
      <div className="w-full max-w-sm rounded-xl border border-border bg-card p-6 shadow-sm space-y-6">
        <div className="space-y-1 text-center">
          <h1 className="text-xl font-bold tracking-tight text-foreground">
            Mockly
          </h1>
          <p className="text-xs text-muted-foreground">
            Aguante Boca Juniors y el equipo de desarrollo de Mockly.
          </p> 
        </div>

        {/* Contador de prueba */}
        <div className="rounded-lg border border-border bg-muted/20 p-4 text-center space-y-3">
          <span className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
            Prueba de contador todo furioso
          </span>
          <div className="text-4xl font-mono font-extrabold text-foreground">
            {count}
          </div>
          <div className="flex items-center justify-center gap-2">
            <button
              type="button"
              onClick={() => setCount((prev) => prev - 1)}
              className="h-8 w-8 inline-flex items-center justify-center rounded-md border border-border bg-background hover:bg-muted text-foreground transition-colors"
              aria-label="Decrementar"
            >
              <Minus className="h-4 w-4" />
            </button>
            <button
              type="button"
              onClick={() => setCount(0)}
              className="h-8 px-2.5 inline-flex items-center justify-center gap-1 rounded-md border border-border bg-background hover:bg-muted text-xs font-medium text-foreground transition-colors"
            >
              <RotateCcw className="h-3 w-3" /> Cortala Pipo
            </button>
            <button
              type="button"
              onClick={() => setCount((prev) => prev + 1)}
              className="h-8 w-8 inline-flex items-center justify-center rounded-md bg-primary hover:bg-primary/90 text-primary-foreground transition-colors"
              aria-label="Incrementar"
            >
              <Plus className="h-4 w-4" />
            </button>
          </div>
        </div>

        {/* Snippet interactivo engau */}
        <div className="space-y-2">
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span>Siguiente comando para consola:</span>
            <button
              type="button"
              onClick={handleCopy}
              className="text-primary hover:underline font-medium inline-flex items-center gap-1"
            >
              {copied ? (
                <>
                  <CheckCircle2 className="h-3 w-3 text-emerald-500" /> Copiado
                </>
              ) : (
                "Copiar"
              )}
            </button>
          </div>
          <code className="block rounded-md border border-border bg-muted/40 px-3 py-2 text-xs font-mono text-foreground select-all">
            sudo shutdown -r now
          </code>
        </div>
      </div>
    </main>
  );
}