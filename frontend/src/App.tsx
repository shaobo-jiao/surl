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
    <ThemeProvider defaultTheme="system" storageKey="vite-ui-theme">
      <BrowserRouter>
        <Routes>
          <Route element={<AuthProvider><AppLayout /></AuthProvider>}>
            <Route index element={<HomePage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route element={<ProtectedRoute />}>
              <Route path="/urls" element={<UrlsPage />} />
              <Route path="/me" element={<ProfilePage />} />
            </Route>
          </Route>
          {/* isolate RedirectionPage from the rest of the app */}
          <Route path="/:shortUrlCode" element={<RedirectionPage />} />
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  );
};

export default App;
