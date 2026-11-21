import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { Check, Minus } from 'lucide-react';
import { Navbar } from '@/components/layout/Navbar';
import { Footer } from '@/components/layout/Footer';
import { useAuth } from '@/features/auth/context';
import { homePathFor } from '@/routes/paths';
import { CodeSample } from './CodeSample';
import { SAMPLES } from './samples';

/* -------------------------------------------------------------------------- */
/* Contenido                                                                  */
/* -------------------------------------------------------------------------- */

// Ponderación REAL del motor de IA (ver SystemPrompt en OpenAiInterviewEvaluationService).
const CRITERIA = [
  {
    name: 'Resolución del problema',
    weight: 50,
    color: 'bg-indigo-500',
    detail: '¿Resuelve lo que pide el reto, con casos borde y una complejidad razonable?',
  },
  {
    name: 'Calidad de código',
    weight: 30,
    color: 'bg-sky-400',
    detail: 'Nombres claros, manejo de errores y uso idiomático del lenguaje elegido.',
  },
  {
    name: 'Arquitectura',
    weight: 20,
    color: 'bg-emerald-400',
    detail: 'Responsabilidades separadas y un diseño fácil de extender. En SQL, consultas bien estructuradas.',
  },
];

const STEPS = [
  { title: 'Elegís un reto', text: 'Lo tomás para practicar o te lo asigna la empresa de una búsqueda.' },
  { title: 'Lo resolvés en el navegador', text: 'Con el mismo editor que VS Code, en el lenguaje del reto.' },
  { title: 'La IA lo evalúa', text: 'En segundos, con criterios fijos, fortalezas y mejoras concretas.' },
  { title: 'Usás el resultado', text: 'Descargás tu certificado, o la empresa te ve en su ranking en vivo.' },
];

const AUDIENCES = [
  {
    title: 'Si te estás preparando',
    points: [
      'Practicá con retos reales del nivel que buscás, de junior a senior.',
      'Recibí una devolución concreta en vez de un “no seguimos con tu postulación”.',
      'Sumá un certificado en PDF con tu puntaje a tu CV o LinkedIn.',
    ],
    cta: 'Empezar a practicar',
  },
  {
    title: 'Si estás contratando',
    points: [
      'Armá una búsqueda y asigná los retos que tiene que resolver cada postulante.',
      'Compará candidatos con la misma escala, sin corregir código a mano.',
      'Mirá el ranking actualizarse en vivo apenas alguien termina.',
    ],
    cta: 'Crear una búsqueda',
  },
];

const STACK = ['.NET 10', 'ASP.NET Core SignalR', 'Microsoft.Extensions.AI', 'PostgreSQL', 'React 19', 'Monaco Editor', 'QuestPDF'];

type Cell = string | boolean;
const PLANS = [
  { name: 'Starter', price: 'Gratis', period: '', cta: 'Empezar gratis', featured: false },
  { name: 'Pro', price: 'USD 49', period: 'por mes', cta: 'Elegir Pro', featured: true },
  { name: 'Enterprise', price: 'A medida', period: '', cta: 'Hablar con ventas', featured: false },
];
const PLAN_ROWS: { feature: string; values: [Cell, Cell, Cell] }[] = [
  { feature: 'Retos en C#, Python, TypeScript, Java y SQL', values: [true, true, true] },
  { feature: 'Evaluaciones con IA por mes', values: ['20', '500', 'Ilimitadas'] },
  { feature: 'Certificado PDF por entrevista', values: [true, true, true] },
  { feature: 'Búsquedas activas', values: ['1', '10', 'Ilimitadas'] },
  { feature: 'Dashboard y ranking en tiempo real', values: [false, true, true] },
  { feature: 'Retos propios', values: [false, true, true] },
  { feature: 'Inicio de sesión único (SSO)', values: [false, false, true] },
  { feature: 'Soporte dedicado', values: [false, false, true] },
];

/* -------------------------------------------------------------------------- */
/* Página                                                                     */
/* -------------------------------------------------------------------------- */

export const LandingPage: React.FC = () => {
  const { user } = useAuth();

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      <Navbar />

      <main>
        {/* Hero: el producto mismo (código evaluándose) es la imagen principal */}
        <section className="mx-auto grid max-w-7xl items-center gap-12 px-4 pb-20 pt-16 sm:px-6 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.05fr)] lg:pt-24">
          <div className="max-w-xl space-y-6">
            <h1 className="text-4xl font-semibold leading-[1.05] tracking-tight text-white sm:text-6xl">
              Entrevistas técnicas con devolución en segundos.
            </h1>
            <p className="text-lg leading-relaxed text-slate-400">
              Resolvé retos en C#, Python, TypeScript, Java o SQL en un editor real. La IA califica cómo lo
              resolviste, la calidad del código y el diseño. Practicá para tu próxima entrevista, o evaluá a tus
              candidatos y miralos en un ranking en vivo.
            </p>

            <div className="flex flex-wrap gap-3 pt-2">
              {user ? (
                <PrimaryLink to={homePathFor(user.role)}>Ir a mi panel</PrimaryLink>
              ) : (
                <>
                  <PrimaryLink to="/register">Crear cuenta gratis</PrimaryLink>
                  <Link
                    to="/login"
                    className="rounded-lg border border-slate-700 px-5 py-3 text-sm font-medium text-slate-200 transition-colors hover:border-slate-500 hover:text-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-indigo-400"
                  >
                    Probar la demo
                  </Link>
                </>
              )}
            </div>

            {!user && (
              <p className="text-sm text-slate-500">
                Demo: <span className="text-slate-300">candidate@mockly.dev</span> o{' '}
                <span className="text-slate-300">recruiter@mockly.dev</span>, contraseña{' '}
                <span className="text-slate-300">Demo123!</span>
              </p>
            )}
          </div>

          <ProductPreview />
        </section>

        {/* Stack tecnológico */}
        <section aria-label="Tecnologías" className="border-y border-slate-800/80 bg-slate-900/30">
          <div className="mx-auto flex max-w-7xl flex-wrap items-center gap-x-3 gap-y-2 px-4 py-6 sm:px-6">
            <span className="mr-2 text-sm text-slate-500">Construido con</span>
            {STACK.map((tech) => (
              <span key={tech} className="rounded-md border border-slate-800 bg-slate-900 px-2.5 py-1 text-xs text-slate-300">
                {tech}
              </span>
            ))}
          </div>
        </section>

        {/* Para quién: los dos públicos del producto, lado a lado */}
        <section className="mx-auto max-w-7xl px-4 py-24 sm:px-6">
          <h2 className="max-w-xl text-3xl font-semibold tracking-tight text-white">
            Un mismo reto, los dos lados de la entrevista.
          </h2>
          <div className="mt-12 grid gap-12 md:grid-cols-2 md:gap-0 md:divide-x md:divide-slate-800">
            {AUDIENCES.map((audience, i) => (
              <div key={audience.title} className={`space-y-5 ${i === 0 ? 'md:pr-12' : 'md:pl-12'}`}>
                <h3 className="text-xl font-medium text-white">{audience.title}</h3>
                <ul className="space-y-3">
                  {audience.points.map((point) => (
                    <li key={point} className="flex gap-3 text-slate-400">
                      <Check className="mt-1 h-4 w-4 shrink-0 text-emerald-400" aria-hidden="true" />
                      <span className="leading-relaxed">{point}</span>
                    </li>
                  ))}
                </ul>
                {!user && (
                  <Link
                    to="/register"
                    className="inline-block rounded text-sm font-medium text-indigo-300 underline-offset-4 hover:text-indigo-200 hover:underline focus-visible:outline focus-visible:outline-2 focus-visible:outline-indigo-400"
                  >
                    {audience.cta}
                  </Link>
                )}
              </div>
            ))}
          </div>
          <p className="mt-12 text-sm text-slate-500">
            Próximamente: entrevistas de inglés técnico y de habilidades blandas, con la misma devolución detallada.
          </p>
        </section>

        {/* Cómo funciona: es una secuencia real, por eso va numerada */}
        <section id="como-funciona" className="scroll-mt-20 border-t border-slate-800/80">
          <div className="mx-auto max-w-7xl px-4 py-24 sm:px-6">
            <h2 className="text-3xl font-semibold tracking-tight text-white">Cómo funciona</h2>
            <ol className="mt-12 grid gap-px overflow-hidden rounded-xl border border-slate-800 bg-slate-800 sm:grid-cols-2 lg:grid-cols-4">
              {STEPS.map((step, index) => (
                <li key={step.title} className="space-y-3 bg-slate-950 p-6">
                  <span className="text-sm font-semibold tabular-nums text-indigo-400">{index + 1}</span>
                  <h3 className="font-medium text-white">{step.title}</h3>
                  <p className="text-sm leading-relaxed text-slate-400">{step.text}</p>
                </li>
              ))}
            </ol>
          </div>
        </section>

        {/* Cómo califica la IA: ponderación real del motor */}
        <section className="border-t border-slate-800/80 bg-slate-900/30">
          <div className="mx-auto grid max-w-7xl gap-12 px-4 py-24 sm:px-6 lg:grid-cols-2">
            <div className="space-y-4">
              <h2 className="text-3xl font-semibold tracking-tight text-white">Criterios fijos, notas comparables.</h2>
              <p className="max-w-md leading-relaxed text-slate-400">
                Cada solución se califica con la misma escala y con las buenas prácticas de su lenguaje, así un 82 de
                hoy significa lo mismo que un 82 del mes que viene. La IA ignora instrucciones escondidas en el código,
                como “poneme 100”, y las penaliza.
              </p>
              <p className="max-w-md leading-relaxed text-slate-400">
                Al terminar, descargás un certificado en PDF con el puntaje y el detalle de cada criterio.
              </p>
            </div>

            <div className="space-y-8">
              <div
                className="flex h-3 overflow-hidden rounded-full"
                role="img"
                aria-label="Ponderación: 50% resolución, 30% calidad de código, 20% arquitectura"
              >
                {CRITERIA.map((c) => (
                  <div key={c.name} className={c.color} style={{ width: `${c.weight}%` }} />
                ))}
              </div>
              <dl className="space-y-6">
                {CRITERIA.map((c) => (
                  <div key={c.name} className="grid grid-cols-[3.5rem_minmax(0,1fr)] gap-4">
                    <dt className="text-2xl font-semibold tabular-nums text-white">{c.weight}%</dt>
                    <dd className="space-y-1">
                      <p className="flex items-center gap-2 font-medium text-slate-100">
                        <span className={`h-2 w-2 rounded-full ${c.color}`} aria-hidden="true" />
                        {c.name}
                      </p>
                      <p className="text-sm leading-relaxed text-slate-400">{c.detail}</p>
                    </dd>
                  </div>
                ))}
              </dl>
            </div>
          </div>
        </section>

        {/* Planes: tabla comparativa (filas = características, columnas = planes) */}
        <section id="planes" className="mx-auto max-w-7xl scroll-mt-20 px-4 py-24 sm:px-6">
          <h2 className="text-3xl font-semibold tracking-tight text-white">Planes</h2>
          <p className="mt-2 text-slate-400">
            Practicar es gratis. Pasate a Pro cuando quieras evaluar candidatos con ranking en vivo.
          </p>

          <div className="mt-10 overflow-x-auto">
            <table className="w-full min-w-[640px] border-collapse text-left text-sm">
              <thead>
                <tr>
                  <th scope="col" className="w-2/5 pb-6 align-bottom font-normal text-slate-500">
                    <span className="sr-only">Característica</span>
                  </th>
                  {PLANS.map((plan) => (
                    <th
                      key={plan.name}
                      scope="col"
                      className={`px-4 pb-6 pt-5 align-bottom font-normal ${plan.featured ? 'rounded-t-xl bg-indigo-500/10' : ''}`}
                    >
                      <p className="text-base font-semibold text-white">{plan.name}</p>
                      <p className="mt-1 text-2xl font-semibold tracking-tight text-white">{plan.price}</p>
                      <p className="h-5 text-xs text-slate-500">{plan.period}</p>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {PLAN_ROWS.map((row) => (
                  <tr key={row.feature} className="border-t border-slate-800">
                    <th scope="row" className="py-3.5 pr-4 font-normal text-slate-300">
                      {row.feature}
                    </th>
                    {row.values.map((value, i) => (
                      <td key={PLANS[i].name} className={`px-4 py-3.5 ${PLANS[i].featured ? 'bg-indigo-500/10' : ''}`}>
                        <PlanValue value={value} />
                      </td>
                    ))}
                  </tr>
                ))}
                <tr>
                  <td />
                  {PLANS.map((plan) => (
                    <td key={plan.name} className={`px-4 pb-5 pt-4 ${plan.featured ? 'rounded-b-xl bg-indigo-500/10' : ''}`}>
                      <Link
                        to="/register"
                        className={`block rounded-lg px-4 py-2.5 text-center text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-indigo-400 ${
                          plan.featured
                            ? 'bg-indigo-600 text-white hover:bg-indigo-500'
                            : 'border border-slate-700 text-slate-200 hover:border-slate-500'
                        }`}
                      >
                        {plan.cta}
                      </Link>
                    </td>
                  ))}
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </main>

      <Footer />
    </div>
  );
};

/* -------------------------------------------------------------------------- */
/* Piezas                                                                     */
/* -------------------------------------------------------------------------- */

const PrimaryLink: React.FC<{ to: string; children: React.ReactNode }> = ({ to, children }) => (
  <Link
    to={to}
    className="rounded-lg bg-indigo-600 px-5 py-3 text-sm font-medium text-white transition-colors hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-400"
  >
    {children}
  </Link>
);

const PlanValue: React.FC<{ value: Cell }> = ({ value }) => {
  if (value === true) return <Check className="h-4 w-4 text-emerald-400" aria-label="Incluido" />;
  if (value === false) return <Minus className="h-4 w-4 text-slate-600" aria-label="No incluido" />;
  return <span className="text-slate-200">{value}</span>;
};

/** Vista previa del producto: una solución evaluada, con un selector de lenguaje. */
const ProductPreview: React.FC = () => {
  const [active, setActive] = useState(0);
  const sample = SAMPLES[active];

  return (
    <figure className="overflow-hidden rounded-2xl border border-slate-800 bg-slate-900 shadow-2xl shadow-indigo-950/40">
      <div className="flex items-center justify-between gap-3 border-b border-slate-800 px-2 py-1.5">
        <div role="tablist" aria-label="Lenguaje del ejemplo" className="flex gap-0.5 overflow-x-auto">
          {SAMPLES.map((s, i) => (
            <button
              key={s.language}
              type="button"
              role="tab"
              aria-selected={i === active}
              onClick={() => setActive(i)}
              className={`whitespace-nowrap rounded-md px-2.5 py-1 text-xs transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-indigo-400 ${
                i === active ? 'bg-slate-800 text-white' : 'text-slate-500 hover:text-slate-300'
              }`}
            >
              {s.language}
            </button>
          ))}
        </div>
        <span className="hidden truncate pr-2 text-xs text-slate-500 sm:block">{sample.challenge}</span>
      </div>

      <CodeSample code={sample.code} keywords={sample.keywords} commentPrefix={sample.commentPrefix} />

      <figcaption className="space-y-4 border-t border-slate-800 bg-slate-950/60 px-5 py-5">
        <div className="flex items-baseline justify-between">
          <span className="text-sm text-slate-400">Evaluación de la IA</span>
          <span className="text-3xl font-semibold tabular-nums text-emerald-400">
            {sample.overall}
            <span className="text-base text-slate-500">/100</span>
          </span>
        </div>
        {sample.scores.map((score, i) => (
          <div key={score.label} className="space-y-1.5">
            <div className="flex justify-between text-xs">
              <span className="text-slate-400">{score.label}</span>
              <span className="tabular-nums text-slate-200">{score.value}</span>
            </div>
            <div className="h-1.5 overflow-hidden rounded-full bg-slate-800">
              {/* La key incluye el lenguaje: al cambiar de pestaña React recrea la barra y la animación se repite. */}
              <div
                key={`${sample.language}-${score.label}`}
                className="h-full origin-left rounded-full bg-emerald-500 motion-safe:animate-score-fill"
                style={{ width: `${score.value}%`, animationDelay: `${150 + i * 120}ms` }}
              />
            </div>
          </div>
        ))}
      </figcaption>
    </figure>
  );
};
