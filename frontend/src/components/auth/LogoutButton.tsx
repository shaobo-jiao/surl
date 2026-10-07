import { useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import { Spinner } from "@/components/ui/spinner";
import { Button } from "@/components/ui/button";


export default function LogoutButton() {
  const [isLoggingOut, setIsLoggingOut] = useState<boolean>(false);
  const { logout } = useAuth();

  const handleClick = async () => {
    setIsLoggingOut(true);
    await logout();
    setIsLoggingOut(false);
  };

  return isLoggingOut
    ? <Button variant="link" disabled> <Spinner /> Logout</Button>
    : <Button variant="link" onClick={handleClick}>Logout</Button>;
}