import { Separator } from "./ui/separator";
import { Button } from "./ui/button";
import { Link, NavLink } from "react-router";
import { useAuth } from "@/contexts/AuthContext";

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
          <Button asChild variant="link"><NavLink to="/">Home</NavLink></Button>

          <Separator orientation="vertical" />
          <Button asChild variant="link"><NavLink to="/urls">URLs</NavLink></Button>

          {
            !isAuthenticated && <>
              <Separator orientation="vertical" />
              <Button asChild variant="link"><NavLink to="/register">Register</NavLink></Button>

              <Separator orientation="vertical" />
              <Button asChild variant="link"><NavLink to="/login">Login</NavLink></Button>
            </>
          }

          {
            isAuthenticated && <>
              <Separator orientation="vertical" />
              {/* to put logout button in a separate component */}
              <Button asChild variant="link"><NavLink to="/logout">Log out</NavLink></Button>

              <Separator orientation="vertical" />
              <Button asChild variant="link"><NavLink to="/me">Hi, {usernameForDisplay}</NavLink></Button>
            </>
          }
        </div>
      </div>
    </header>
  );
};

export default Header;