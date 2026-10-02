import Header from "@/components/Header";
import { Outlet } from "react-router";
import { Toaster } from "sonner";


const AppLayout = () => {
  return (
    <>
      <Header />
      <main className="flex flex-col w-full min-h-screen items-center p-4">
        <Outlet />
      </main>
      <Toaster />
    </>
  )
};

export default AppLayout;