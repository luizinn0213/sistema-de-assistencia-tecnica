import axios from 'axios';

export const api = axios.create({
  baseURL: 'http://localhost:5000/api'
});

export function pegarErro(err: any): string {
  return err?.response?.data?.erro ?? 'Erro inesperado.';
}