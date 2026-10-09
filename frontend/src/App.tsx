import AppLayout from "@/AppLayout";
import HomePage from "@/pages/HomePage";
import RedirectionPage from "@/pages/RedirectionPage";
import UrlsPage from "@/pages/UrlsPage";
import { Route } from "react-router";
import { BrowserRouter, Routes } from "react-router";
import RegisterPage from "./pages/RegisterPage";
import LoginPage from "./pages/LoginPage";
import ProtectedRoute from "./components/auth/ProtectedRoute";
import ProfilePage from "./pages/ProfilePage";
import AuthProvider from "./components/auth/AuthProvider";
import ThemeProvider from "./components/common/ThemeProvider";


const App = () => {
  return (
    <ThemeProvider defaultTheme="light" storageKey="vite-ui-theme">
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route element={<AppLayout />}>
              <Route index element={<HomePage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/:shortUrlCode" element={<RedirectionPage />} />
              <Route element={<ProtectedRoute />}>
                <Route path="/urls" element={<UrlsPage />} />
                <Route path="/me" element={<ProfilePage />} />
              </Route>
            </Route>
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
};

export default App;
