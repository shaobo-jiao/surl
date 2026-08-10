import { Separator } from "./ui/separator";
import { Button } from "./ui/button";

// Header contains a "Logo" that links to homepage and a nav bar 
// the nav bar contains: Home, URLs only as for now. later need to add Login/Logout links
const Header = () => {
  // todo: user login info related visibility control; screen width related control
  return (
    <header className="flex justify-center h-16 border-1 p-4">
      <div className="flex items-center gap-2 w-5xl">
        <a href="/" className="text-3xl font-black">SURL</a>

        <div className="flex ml-auto">
          <Button asChild variant="link"><a href="/" className="font-medium h-auto">Home</a></Button>

          <Separator orientation="vertical" />
          <Button asChild variant="link"><a href="/urls" className="font-medium h-auto">URLs</a></Button>

          <Separator orientation="vertical" />
          <Button asChild variant="link"><a href="/register" className="font-medium h-auto">Register</a></Button>

          <Separator orientation="vertical" />
          <Button asChild variant="link"><a href="/login" className="font-medium h-auto">Login</a></Button>
        </div>
      </div>
    </header>
  );
};

export default Header;