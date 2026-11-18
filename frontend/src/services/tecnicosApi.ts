import { apiGet } from "@/lib/api";
import type { Tecnico } from "@/types/Tecnico";

// Mesma forma de resposta do controller .NET api/tecnicos; caminho relativo /api
// pela camada tipada do template (proxy do Vite em dev, mesma origem em produção).
export const listarTecnicos = () => apiGet<Tecnico[]>("/tecnicos");
