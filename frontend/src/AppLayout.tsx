import AppHeader from "@/components/AppHeader";
import AppFooter from "@/components/AppFooter";
import { Outlet } from "react-router";
import { Toaster } from "sonner";


const AppLayout = () => {
  return (
    <div className="flex flex-col min-h-screen">
      <AppHeader />
      <main className="flex flex-1 flex-col items-center w-full p-4">
        <Outlet />
      </main>
      <AppFooter />
      <Toaster />
    </div>
  )
};

export default AppLayout;
