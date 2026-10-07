import AppHeader from "@/components/AppHeader";
import { Outlet } from "react-router";
import { Toaster } from "sonner";


const AppLayout = () => {
  return (
    <>
      <AppHeader />
      <main className="flex flex-col w-full min-h-screen items-center p-4">
        <Outlet />
      </main>
      <Toaster />
    </>
  )
};

export default AppLayout;