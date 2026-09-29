import { useEffect } from 'react'

interface ToastProps {
  mensagem: string
  tipo: 'sucesso' | 'erro'
  aoFechar: () => void
}

export function Toast({
  mensagem,
  tipo,
  aoFechar
}: ToastProps) {
  useEffect(() => {
    const temporizador = window.setTimeout(() => {
      aoFechar()
    }, 3600)

    return () => {
      window.clearTimeout(temporizador)
    }
  }, [mensagem, tipo, aoFechar])

  return (
    <div
      className={`toast ${tipo}`}
      role={tipo === 'erro' ? 'alert' : 'status'}
    >
      {mensagem}
    </div>
  )
}