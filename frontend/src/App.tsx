import AppLayout from "@/AppLayout";
import HomePage from "@/pages/HomePage";
import RedirectionPage from "@/pages/RedirectionPage";
import UrlsPage from "@/pages/UrlsPage";
import { Route } from "react-router";
import { BrowserRouter, Routes } from "react-router";
import RegisterPage from "./pages/RegisterPage";
import LoginPage from "./pages/LoginPage";


const App = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppLayout />}>
          <Route index element={<HomePage />} />
          <Route path="/urls" element={<UrlsPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/:shortUrlCode" element={<RedirectionPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
};

export default App;
