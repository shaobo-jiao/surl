import { useState } from "react";
import { Link, NavLink } from "react-router";
import { Menu } from "lucide-react";
import { Button, buttonVariants } from "./ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "./ui/popover";
import { useAuth } from "@/hooks/useAuth";
import LogoutButton from "./auth/LogoutButton";

// Header contains a "Logo" that links to homepage and a nav bar 
// the nav bar contains: Home, URLs only as for now. later need to add Login/Logout links
const AppHeader = () => {
  const { userLogin } = useAuth();
  const isAuthenticated = userLogin.isAuthenticated;
  const [isMenuOpen, setIsMenuOpen] = useState<boolean>(false);

  let usernameForDisplay = userLogin.username;
  if (usernameForDisplay !== null && usernameForDisplay.length > 15)
    usernameForDisplay = usernameForDisplay.substring(0, 12) + "...";

  // close the small-screen menu after a navigation item is clicked
  const closeMenu = () => setIsMenuOpen(false);

  // the nav items are shared so the desktop bar and the small-screen menu stay in sync
  const renderNavItems = (itemClassName: string) => (
    <>
      <NavLink to="/" className={itemClassName} onClick={closeMenu}>Home</NavLink>
      <NavLink to="/urls" className={itemClassName} onClick={closeMenu}>URLs</NavLink>

      {
        !isAuthenticated && <>
          <NavLink to="/register" className={itemClassName} onClick={closeMenu}>Register</NavLink>
          <NavLink to="/login" className={itemClassName} onClick={closeMenu}>Login</NavLink>
        </>
      }

      {
        isAuthenticated && <>
          <LogoutButton className={itemClassName} onClick={closeMenu} />
          <NavLink to="/me" className={itemClassName} onClick={closeMenu}>Hi, {usernameForDisplay}</NavLink>
        </>
      }
    </>
  );
  const desktopItemClassName = buttonVariants({ variant: "link" });
  const menuItemClassName = buttonVariants({ variant: "link", className: "w-full justify-start" });

  return (
    <header className="w-full border-b">
      <div className="mx-auto flex flex-row items-center justify-between gap-2 p-4 w-full max-w-5xl">
        <Link to="/" className="text-3xl font-black" onClick={closeMenu}>SURL</Link>

        {/* wide screens: inline nav bar */}
        <nav className="hidden items-center sm:flex">
          {renderNavItems(desktopItemClassName)}
        </nav>

        {/* small screens: nav items collapsed behind a menu icon popover */}
        <Popover open={isMenuOpen} onOpenChange={setIsMenuOpen}>
          <PopoverTrigger
            render={
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="sm:hidden"
                aria-label="Open navigation menu" />}>
            <Menu />
          </PopoverTrigger>

          <PopoverContent align="end" className="sm:hidden w-48 gap-0.5 p-2">
            {renderNavItems(menuItemClassName)}
          </PopoverContent>
        </Popover>
      </div>
    </header>
  );
};

export default AppHeader;
