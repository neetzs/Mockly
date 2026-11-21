import React, { useState } from 'react';
import { Download, Loader2 } from 'lucide-react';
import { useAuth } from '../../auth/context';
import { interviewService } from '../services/interview.service';

/**
 * Descarga el certificado PDF de una entrevista completada.
 * Flujo: fetch con JWT → Blob → URL temporal en memoria → <a download> invisible → click → liberar URL.
 */
export const CertificateDownloadButton: React.FC<{ interviewId: string }> = ({ interviewId }) => {
  const { token } = useAuth();
  const [isDownloading, setIsDownloading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleDownload = async () => {
    if (!token) return;
    setIsDownloading(true);
    setError(null);

    try {
      const { blob, fileName } = await interviewService.downloadCertificate(interviewId, token);

      // createObjectURL crea una URL "blob:..." que apunta al archivo en memoria del navegador.
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = fileName;
      link.click();

      // Liberamos la memoria: la URL ya no hace falta una vez iniciada la descarga.
      URL.revokeObjectURL(url);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'No se pudo descargar el certificado.');
    } finally {
      setIsDownloading(false);
    }
  };

  return (
    <div className="space-y-2">
      <button
        type="button"
        onClick={handleDownload}
        disabled={isDownloading}
        className="inline-flex w-full items-center justify-center gap-2 rounded-lg border border-indigo-500/40 bg-indigo-500/10 hover:bg-indigo-500/20 disabled:opacity-60 px-4 py-2.5 text-sm font-semibold text-brand transition-colors"
      >
        {isDownloading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Download className="h-4 w-4" />}
        {isDownloading ? 'Generando certificado…' : 'Descargar certificado PDF'}
      </button>
      {error && <p className="text-xs text-danger">{error}</p>}
    </div>
  );
};
