import { NavigationMenu, NavigationMenuItem, NavigationMenuLink, NavigationMenuList } from "./ui/navigation-menu";

// Header contains a "Logo" that links to homepage and a nav bar 
// the nav bar contains: Home, URLs only as for now. later need to add Login/Logout links
const Header = () => {
  // todo: user login info related visibility control; screen width related control
  return (
    <header className="flex justify-center h-16 border-1 p-4">
      <div className="flex items-center gap-2 w-5xl">
        <a href="/" className="text-3xl font-black">SURL</a>
        <NavigationMenu className="ml-auto">
          <NavigationMenuList>
            <NavigationMenuItem>
              <NavigationMenuLink asChild>
                <a href="/" className="font-medium">Home</a>
              </NavigationMenuLink>
            </NavigationMenuItem>
            <NavigationMenuItem>
              <NavigationMenuLink asChild>
                <a href="/urls" className="font-medium">URLs</a>
              </NavigationMenuLink>
            </NavigationMenuItem>
          </NavigationMenuList>
        </NavigationMenu>
        <h4 className="text-sm font-medium italic">Hi, XXX</h4>
      </div>
    </header>
  );
};

export default Header;