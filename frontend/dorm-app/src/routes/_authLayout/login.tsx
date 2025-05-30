import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { authApi } from "@/lib/api";
import { Label } from "@radix-ui/react-label";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createFileRoute, Link, useNavigate, useSearch } from "@tanstack/react-router";
import { Field, Form, Formik } from "formik";
import { useState } from "react";
import * as Yup from "yup";

export const Route = createFileRoute("/_authLayout/login")({
  component: RouteComponent,
  validateSearch: (search: Record<string, unknown>) => ({
    returnTo: (search.returnTo as string) || "/",
  }),
});

// Login validation schema with Yup
const loginSchema = Yup.object().shape({
  email: Yup.string()
    .email("Please enter a valid email address")
    .required("Email is required"),
  password: Yup.string()
    .min(6, "Password must be at least 6 characters")
    .required("Password is required"),
});

// Initial form values
const initialValues = {
  email: "",
  password: "",
};

function RouteComponent() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const queryClient = useQueryClient();
  
  // Get the returnTo parameter from the URL
  const search = useSearch({ from: "/_authLayout/login" });
  const returnTo = search.returnTo || "/";

  // Use TanStack Query for login API call
  const loginMutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: () => {
      // Token is already stored in localStorage by the login function
      // Invalidate the auth status query to trigger a refresh of navbar
      queryClient.invalidateQueries({ queryKey: ["authStatus"] });
      
      // Redirect to returnTo path or home as fallback
      navigate({ to: returnTo });
    },
    onError: (error: Error) => {
      console.error("Login error details:", error);

      // Display raw error message, especially useful for 404 errors
      setError(error.message || "Failed to connect to authentication server");
    },
  });

  const handleSubmit = (values: typeof initialValues) => {
    setError(null);
    loginMutation.mutate(values);
  };

  return (
    <div className="flex justify-center items-center min-h-screen bg-background">
      <Card className="w-[350px]">
        <CardHeader className="text-center">
          {/* Logo */}
          <div className="flex justify-center mb-4">
            <img 
              src="/WhiteLogo.svg" 
              alt="Dorm System Logo" 
              className="h-16 w-16"
            />
          </div>
          
          <CardTitle>Login</CardTitle>
          <CardDescription>
            Enter your credentials to access your account
            {returnTo !== "/" && (
              <p className="text-xs text-muted-foreground mt-1">
                You'll be redirected back after login
              </p>
            )}
          </CardDescription>
        </CardHeader>
        <Formik
          initialValues={initialValues}
          validationSchema={loginSchema}
          onSubmit={handleSubmit}
        >
          {({ errors, touched }) => (
            <Form>
              <CardContent className="space-y-4">
                {error && (
                  <div className="bg-destructive/20 p-3 rounded text-destructive text-sm overflow-auto max-h-32">
                    <p className="font-semibold mb-1">Error:</p>
                    {error}
                  </div>
                )}
                <div className="space-y-2">
                  <Label htmlFor="email">Email</Label>
                  <Field
                    as={Input}
                    id="email"
                    name="email"
                    type="email"
                    placeholder="name@example.com"
                  />
                  <div className="h-5">
                    {errors.email && touched.email && (
                      <p className="text-sm text-destructive">{errors.email}</p>
                    )}
                  </div>
                </div>
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <Label htmlFor="password">Password</Label>
                    {/* Removed forgot password link to fix routing error */}
                  </div>
                  <Field
                    as={Input}
                    id="password"
                    name="password"
                    type="password"
                  />
                  <div className="h-5">
                    {errors.password && touched.password && (
                      <p className="text-sm text-destructive">
                        {errors.password}
                      </p>
                    )}
                  </div>
                </div>
              </CardContent>
              <CardFooter className="flex flex-col space-y-4">
                <Button
                  type="submit"
                  className="w-full"
                  disabled={loginMutation.isPending}
                >
                  {loginMutation.isPending ? "Signing in..." : "Sign In"}
                </Button>
                <p className="text-center text-sm">
                  Don't have an account?{" "}
                  <Link to="/register" className="text-primary hover:underline">
                    Register
                  </Link>
                </p>
              </CardFooter>
            </Form>
          )}
        </Formik>
      </Card>
    </div>
  );
}
