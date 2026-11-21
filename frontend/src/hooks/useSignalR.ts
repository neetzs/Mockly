import { useEffect, useRef, useState } from 'react';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';

export type ConnectionStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

// Cada evento del hub se mapea a una función que recibe su payload.
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type HubHandlers = Record<string, (...args: any[]) => void>;

/**
 * Conecta a un hub de SignalR mientras el componente está montado.
 *
 * - Envía el JWT con accessTokenFactory: SignalR lo manda como ?access_token=... en el WebSocket
 *   (el backend lo acepta solo en /hubs, ver OnMessageReceived en Program.cs).
 * - withAutomaticReconnect: si se corta la red, reintenta solo (0s, 2s, 10s, 30s).
 * - Devuelve el estado de la conexión para mostrar "En vivo" / "Reconectando…".
 */
export function useSignalR(hubUrl: string, token: string | null, handlers: HubHandlers): ConnectionStatus {
  const [status, setStatus] = useState<ConnectionStatus>('connecting');

  // Guardamos los handlers en un ref: así el componente puede pasar funciones nuevas en cada render
  // SIN que tengamos que cerrar y reabrir la conexión (el efecto principal no depende de ellos).
  const handlersRef = useRef(handlers);
  useEffect(() => {
    handlersRef.current = handlers;
  });

  useEffect(() => {
    if (!token) return;

    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    // Registramos un "puente" por evento que siempre llama al handler más reciente.
    for (const eventName of Object.keys(handlersRef.current)) {
      connection.on(eventName, (...args) => handlersRef.current[eventName]?.(...args));
    }

    connection.onreconnecting(() => setStatus('reconnecting'));
    connection.onreconnected(() => setStatus('connected'));
    connection.onclose(() => setStatus('disconnected'));

    // isActive = false cuando el efecto ya se limpió. En desarrollo, StrictMode monta → desmonta → monta,
    // así que la primera conexión se corta a propósito: ese error no lo mostramos.
    let isActive = true;

    connection
      .start()
      .then(() => {
        if (isActive && connection.state === HubConnectionState.Connected) setStatus('connected');
      })
      .catch(() => {
        if (isActive) setStatus('disconnected');
      });

    // Cleanup: al salir del dashboard (o si cambia el token) cerramos el WebSocket.
    return () => {
      isActive = false;
      void connection.stop();
    };
  }, [hubUrl, token]);

  return status;
}
