import { useAuth } from "@/hooks/useAuth";
import { Navigate, Outlet, useLocation } from "react-router";


export default function ProtectedRoute() {
  const { userLogin } = useAuth();
  const location = useLocation();

  if (!userLogin.isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }
  
  return <Outlet />
}