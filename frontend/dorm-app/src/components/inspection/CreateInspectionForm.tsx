"use client";

import type React from "react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { Inspection } from "@/lib/types/inspection";
import { ArrowLeft } from "lucide-react";
import { useState } from "react";

interface CreateInspectionFormProps {
  onSubmit: (inspection: Omit<Inspection, "id" | "rooms">) => void;
  onCancel: () => void;
}

export function CreateInspectionForm({
  onSubmit,
  onCancel,
}: CreateInspectionFormProps) {
  const [name, setName] = useState("");
  const [type, setType] = useState("");
  const [date, setDate] = useState("");
  const [time, setTime] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Combine date and time into a Date object
    const [year, month, day] = date.split("-").map(Number);
    const [hours, minutes] = time.split(":").map(Number);

    const startDate = new Date(year, month - 1, day, hours, minutes);

    onSubmit({
      name,
      type,
      startDate,
      status: "scheduled",
    });
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center space-x-2">
        <Button variant="ghost" size="sm" onClick={onCancel}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Inspections
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Create New Inspection</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Inspection Name</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter inspection name"
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="type">Inspection Type</Label>
              <Select value={type} onValueChange={setType} required>
                <SelectTrigger id="type">
                  <SelectValue placeholder="Select inspection type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Safety">Safety</SelectItem>
                  <SelectItem value="Maintenance">Maintenance</SelectItem>
                  <SelectItem value="Fire Safety">Fire Safety</SelectItem>
                  <SelectItem value="Health">Health</SelectItem>
                  <SelectItem value="General">General</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="date">Date</Label>
                <Input
                  id="date"
                  type="date"
                  value={date}
                  onChange={(e) => setDate(e.target.value)}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="time">Time</Label>
                <Input
                  id="time"
                  type="time"
                  value={time}
                  onChange={(e) => setTime(e.target.value)}
                  required
                />
              </div>
            </div>

            <div className="flex justify-end space-x-2 pt-4">
              <Button type="button" variant="outline" onClick={onCancel}>
                Cancel
              </Button>
              <Button type="submit">Create Inspection</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
