import { Route, Routes } from "react-router-dom";
import { Toaster } from "@/components/ui/sonner";
import Header from "@/components/Header";
import ListaTecnicos from "@/pages/ListaTecnicos";
import Solicitacoes from "@/pages/Solicitacoes";

// Uma <Route> por página em src/pages; BrowserRouter já envolve o App em main.tsx.
export default function App() {
  return (
    <>
      <Header />

      <Routes>
        <Route path="/" element={<ListaTecnicos />} />
        <Route path="/solicitacoes" element={<Solicitacoes />} />
      </Routes>

      <Toaster />
    </>
  );
}
