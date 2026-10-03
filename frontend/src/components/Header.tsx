import { buttonVariants } from "./ui/button";
import { Link, NavLink } from "react-router";
import LogoutButton from "./LogoutButton";
import { useAuth } from "@/hooks/useAuth";

// Header contains a "Logo" that links to homepage and a nav bar 
// the nav bar contains: Home, URLs only as for now. later need to add Login/Logout links
const Header = () => {  
  const { userLogin } = useAuth();
  const isAuthenticated = userLogin.isAuthenticated;

  let usernameForDisplay = userLogin.username;
  if (usernameForDisplay !== null && usernameForDisplay.length > 15)
    usernameForDisplay = usernameForDisplay.substring(0, 12) + "...";

  return (
    <header className="flex justify-center h-16 border-1 p-4">
      <div className="flex items-center gap-2 w-5xl">
        <Link to="/" className="text-3xl font-black">SURL</Link>

        <div className="flex ml-auto">
          <NavLink to="/" className={buttonVariants({ variant: "link" })}>Home</NavLink>
          <NavLink to="/urls" className={buttonVariants({ variant: "link" })}>URLs</NavLink>

          {
            !isAuthenticated && <>
              <NavLink to="/register" className={buttonVariants({ variant: "link" })}>Register</NavLink>
              <NavLink to="/login" className={buttonVariants({ variant: "link" })}>Login</NavLink>
            </>
          }

          {
            isAuthenticated && <>
              <LogoutButton />
              <NavLink to="/me" className={buttonVariants({ variant: "link" })}>Hi, {usernameForDisplay}</NavLink>
            </>
          }
        </div>
      </div>
    </header>
  );
};

export default Header;
