import ChangePasswordFormCard from "@/components/auth/ChangePasswordFormCard";
import useAuth from "@/hooks/useAuth";

export default function ProfilePage() {
  const { userLogin } = useAuth();

  return (
    <div className="flex flex-col items-center w-full max-w-2xl gap-4 mt-4">

      {/* simple account header */}
      <div className="flex flex-col w-full gap-1">
        <h1 className="font-bold text-2xl">Hi, {userLogin.username}</h1>
        <p className="text-muted-foreground">Manage your account settings below</p>
      </div>

      <ChangePasswordFormCard />
    </div>
  );
}
