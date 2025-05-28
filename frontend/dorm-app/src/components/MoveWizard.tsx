import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useBuildings } from '@/lib/hooks/useBuildings';
import { useMoveIn, useMoveOut, usePlaces } from '@/lib/hooks/usePlaces';
import { useRooms } from '@/lib/hooks/useRooms';
import { UserDetails, useUsers } from '@/lib/hooks/useUser';
import { PlaceResponse } from '@/lib/types/place';
import { getPlaceholderAvatar } from '@/lib/utils';
import {
  AlertCircle,
  Bed,
  Building,
  CheckCircle,
  Loader2,
  LogIn,
  LogOut,
  User,
  Users
} from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

type OperationType = 'move-in' | 'move-out';

interface MoveInState {
  selectedBuildingId: string;
  selectedRoomId: string;
  selectedPlaceId: string;
  selectedUserId: string;
}

interface MoveOutState {
  selectedPlaceId: string;
}

export function MoveWizard() {
  const [activeTab, setActiveTab] = useState<OperationType>('move-in');
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  
  // Move-in state
  const [moveInState, setMoveInState] = useState<Partial<MoveInState>>({});
  
  // Move-out state  
  const [moveOutState, setMoveOutState] = useState<Partial<MoveOutState>>({});

  // API hooks
  const { data: buildings } = useBuildings();
  const { data: rooms } = useRooms(moveInState.selectedBuildingId, !!moveInState.selectedBuildingId);
  const { data: availablePlaces } = usePlaces({ 
    roomId: moveInState.selectedRoomId,
    isOccupied: false 
  });
  const { data: allPlaces } = usePlaces({});

  // Filter occupied places on client side
  const occupiedPlaces = {
    ...allPlaces,
    items: allPlaces?.items?.filter(place => place.occupiedByUserId !== null) || []
  };



  const { data: users } = useUsers({ pageSize: 100 });
  
  const { mutate: moveIn, isPending: isMovingIn } = useMoveIn();
  const { mutate: moveOut, isPending: isMovingOut } = useMoveOut();

  // Reset state when switching tabs
  const handleTabChange = (value: string) => {
    const tabValue = value as OperationType;
    setActiveTab(tabValue);
    setMoveInState({});
    setMoveOutState({});
    setShowConfirmDialog(false);
  };

  // Move-in handlers
  const handleBuildingSelect = (buildingId: string) => {
    setMoveInState(prev => ({ 
      ...prev, 
      selectedBuildingId: buildingId,
      selectedRoomId: '',
      selectedPlaceId: '',
      selectedUserId: ''
    }));
  };

  const handleRoomSelect = (roomId: string) => {
    setMoveInState(prev => ({ 
      ...prev, 
      selectedRoomId: roomId,
      selectedPlaceId: '',
      selectedUserId: ''
    }));
  };

  const handlePlaceSelect = (placeId: string) => {
    setMoveInState(prev => ({ ...prev, selectedPlaceId: placeId }));
  };

  const handleUserSelectForMoveIn = (userId: string) => {
    setMoveInState(prev => ({ ...prev, selectedUserId: userId }));
  };

  // Move-out handlers
  const handleOccupiedPlaceSelect = (place: PlaceResponse) => {
    setMoveOutState({
      selectedPlaceId: place.id
    });
  };

  // Confirmation and execution
  const handleMoveInConfirm = () => {
    if (!moveInState.selectedPlaceId || !moveInState.selectedUserId) {
      toast.error('Please select both a place and a user');
      return;
    }

    moveIn(
      { 
        id: moveInState.selectedPlaceId, 
        userId: moveInState.selectedUserId 
      },
      {
        onSuccess: () => {
          setShowConfirmDialog(false);
          setMoveInState({});
          toast.success('User moved in successfully!');
        },
        onError: (error) => {
          toast.error('Failed to move user in. Please try again.');
          console.error('Move in error:', error);
        }
      }
    );
  };

  const handleMoveOutConfirm = () => {
    if (!moveOutState.selectedPlaceId) {
      toast.error('Please select a user to move out');
      return;
    }

    moveOut(
      { 
        id: moveOutState.selectedPlaceId
      },
      {
        onSuccess: () => {
          setShowConfirmDialog(false);
          setMoveOutState({});
          toast.success('User moved out successfully!');
        },
        onError: (error) => {
          toast.error('Failed to move user out. Please try again.');
          console.error('Move out error:', error);
        }
      }
    );
  };

  // Helper functions
  const getSelectedUser = (userId: string): UserDetails | undefined => {
    return users?.items.find(user => user.id === userId);
  };

  const getUserForPlace = (place?: PlaceResponse): UserDetails | undefined => {
    if (!place || !place.occupiedByUserId) return undefined;
    return users?.items.find(user => user.id === place.occupiedByUserId);
  };

  const getSelectedPlace = (placeId: string): PlaceResponse | undefined => {
    return allPlaces?.items?.find(place => place.id === placeId);
  };

  const getSelectedRoom = (roomId: string) => {
    return rooms?.find(room => room.id === roomId);
  };

  const canMoveIn = moveInState.selectedPlaceId && moveInState.selectedUserId;
  const canMoveOut = moveOutState.selectedPlaceId;

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Users className="h-5 w-5" />
            Student Move Management
          </CardTitle>
          <p className="text-sm text-muted-foreground">
            Manage student move-ins and move-outs for dormitory places
          </p>
        </CardHeader>
        <CardContent>
          <Tabs value={activeTab} onValueChange={handleTabChange} className="w-full">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="move-in" className="flex items-center gap-2">
                <LogIn className="h-4 w-4" />
                Move In
              </TabsTrigger>
              <TabsTrigger value="move-out" className="flex items-center gap-2">
                <LogOut className="h-4 w-4" />
                Move Out
              </TabsTrigger>
            </TabsList>

            {/* Move In Tab */}
            <TabsContent value="move-in" className="space-y-6">
              <div className="grid gap-6 md:grid-cols-2">
                {/* Step 1: Select Building */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2 text-sm">
                      <Building className="h-4 w-4" />
                      1. Select Building
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <Select 
                      value={moveInState.selectedBuildingId || ''} 
                      onValueChange={handleBuildingSelect}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Choose a building" className="truncate" />
                      </SelectTrigger>
                      <SelectContent>
                        {buildings?.map((building) => (
                          <SelectItem key={building.id} value={building.id}>
                            <div className="truncate max-w-[300px]">
                              {building.name}
                            </div>
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </CardContent>
                </Card>

                {/* Step 2: Select Room */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2 text-sm">
                      <Bed className="h-4 w-4" />
                      2. Select Room
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <Select 
                      value={moveInState.selectedRoomId || ''} 
                      onValueChange={handleRoomSelect}
                      disabled={!moveInState.selectedBuildingId}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Choose a room" className="truncate" />
                      </SelectTrigger>
                      <SelectContent>
                        {rooms?.map((room) => (
                          <SelectItem key={room.id} value={room.id}>
                            <div className="truncate max-w-[300px]">
                              Room {room.label} ({room.capacity} places)
                            </div>
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </CardContent>
                </Card>

                {/* Step 3: Select Place */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2 text-sm">
                      <Bed className="h-4 w-4" />
                      3. Select Available Place
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    {!moveInState.selectedRoomId ? (
                      <p className="text-sm text-muted-foreground">
                        Please select a room first
                      </p>
                    ) : (
                      <div className="space-y-2">
                        {availablePlaces?.items.length === 0 ? (
                          <p className="text-sm text-muted-foreground">
                            No available places in this room
                          </p>
                        ) : (
                          availablePlaces?.items.map((place) => (
                            <div
                              key={place.id}
                              className={`p-3 border rounded-lg cursor-pointer transition-colors ${
                                moveInState.selectedPlaceId === place.id
                                  ? 'border-primary bg-primary/5'
                                  : 'border-border hover:border-primary/50'
                              }`}
                              onClick={() => handlePlaceSelect(place.id)}
                            >
                              <div className="flex items-center justify-between">
                                <span className="font-medium">Place {place.index}</span>
                                <Badge variant="outline" className="bg-green-100 text-green-800">
                                  Available
                                </Badge>
                              </div>
                            </div>
                          ))
                        )}
                      </div>
                    )}
                  </CardContent>
                </Card>

                {/* Step 4: Select Student */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2 text-sm">
                      <User className="h-4 w-4" />
                      4. Select Student
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <Select 
                      value={moveInState.selectedUserId || ''} 
                      onValueChange={handleUserSelectForMoveIn}
                      disabled={!moveInState.selectedPlaceId}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Choose a student" className="truncate" />
                      </SelectTrigger>
                      <SelectContent>
                        {users?.items.map((user) => (
                          <SelectItem key={user.id} value={user.id}>
                            <div className="truncate max-w-[300px]">
                              {user.firstName} {user.lastName} ({user.email})
                            </div>
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </CardContent>
                </Card>
              </div>

              {/* Move In Summary and Confirm */}
              {canMoveIn && (
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2 text-sm">
                      <CheckCircle className="h-4 w-4" />
                      Summary
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-3">
                      <div>
                        <p className="text-sm font-medium">Student:</p>
                        <p className="text-sm text-muted-foreground truncate">
                          {getSelectedUser(moveInState.selectedUserId!)?.firstName} {getSelectedUser(moveInState.selectedUserId!)?.lastName}
                        </p>
                      </div>
                      <div>
                        <p className="text-sm font-medium">Place:</p>
                        <p className="text-sm text-muted-foreground">
                          Place {getSelectedPlace(moveInState.selectedPlaceId!)?.index} in Room {getSelectedRoom(moveInState.selectedRoomId!)?.label}
                        </p>
                      </div>
                      <Separator />
                      <Button 
                        onClick={() => setShowConfirmDialog(true)} 
                        className="w-full"
                        disabled={isMovingIn}
                      >
                        {isMovingIn ? (
                          <>
                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            Moving In...
                          </>
                        ) : (
                          <>
                            <LogIn className="mr-2 h-4 w-4" />
                            Confirm Move In
                          </>
                        )}
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              )}
            </TabsContent>

            {/* Move Out Tab */}
            <TabsContent value="move-out" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2 text-sm">
                    <User className="h-4 w-4" />
                    Select Student to Move Out
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    {!occupiedPlaces.items?.length ? (
                      <p className="text-sm text-muted-foreground">
                        No occupied places found
                      </p>
                    ) : (
                      occupiedPlaces.items.map((place) => (
                        <div
                          key={place.id}
                          className={`p-4 border rounded-lg cursor-pointer transition-colors ${
                            moveOutState.selectedPlaceId === place.id
                              ? 'border-primary bg-primary/5'
                              : 'border-border hover:border-primary/50'
                          }`}
                          onClick={() => handleOccupiedPlaceSelect(place)}
                        >
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-3">
                              <Avatar className="h-10 w-10">
                                <AvatarFallback>{getPlaceholderAvatar()}</AvatarFallback>
                              </Avatar>
                              <div className="flex-1 min-w-0">
                                <p className="font-medium truncate">
                                  {getUserForPlace(place)?.firstName || 'Unknown'} {getUserForPlace(place)?.lastName || 'Student'}
                                </p>
                                <p className="text-sm text-muted-foreground truncate">
                                  {getUserForPlace(place)?.email || 'No email available'}
                                </p>
                                <p className="text-xs text-muted-foreground">
                                  Place {place.index} • Moved in: {place.movedInAt ? new Date(place.movedInAt).toLocaleDateString() : 'Unknown'}
                                </p>
                              </div>
                            </div>
                            <Badge variant="secondary">
                              Occupied
                            </Badge>
                          </div>
                        </div>
                      ))
                    )}
                  </div>
                </CardContent>
              </Card>

              {/* Move Out Summary and Confirm */}
              {canMoveOut && (
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2 text-sm">
                      <AlertCircle className="h-4 w-4" />
                      Move Out Summary
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-3">
                      <div>
                        <p className="text-sm font-medium">Student:</p>
                        <p className="text-sm text-muted-foreground truncate">
                          {getUserForPlace(getSelectedPlace(moveOutState.selectedPlaceId!)!)?.firstName || 'Unknown'} {getUserForPlace(getSelectedPlace(moveOutState.selectedPlaceId!)!)?.lastName || 'Student'}
                        </p>
                      </div>
                      <div>
                        <p className="text-sm font-medium">Current Place:</p>
                        <p className="text-sm text-muted-foreground">
                          Place {getSelectedPlace(moveOutState.selectedPlaceId!)?.index}
                        </p>
                      </div>
                      <Separator />
                      <Button 
                        onClick={() => setShowConfirmDialog(true)} 
                        variant="destructive" 
                        className="w-full"
                        disabled={isMovingOut}
                      >
                        {isMovingOut ? (
                          <>
                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            Moving Out...
                          </>
                        ) : (
                          <>
                            <LogOut className="mr-2 h-4 w-4" />
                            Confirm Move Out
                          </>
                        )}
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              )}
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      {/* Confirmation Dialog */}
      <Dialog open={showConfirmDialog} onOpenChange={setShowConfirmDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {activeTab === 'move-in' ? 'Confirm Move In' : 'Confirm Move Out'}
            </DialogTitle>
            <DialogDescription>
              {activeTab === 'move-in' 
                ? 'Are you sure you want to move this student into the selected place?' 
                : 'Are you sure you want to move this student out of their current place?'
              }
            </DialogDescription>
          </DialogHeader>
          <div className="py-4">
            {activeTab === 'move-in' ? (
              <div className="space-y-2">
                <p className="truncate"><strong>Student:</strong> {getSelectedUser(moveInState.selectedUserId!)?.firstName} {getSelectedUser(moveInState.selectedUserId!)?.lastName}</p>
                <p><strong>Place:</strong> Place {getSelectedPlace(moveInState.selectedPlaceId!)?.index}</p>
                <p><strong>Room:</strong> {getSelectedRoom(moveInState.selectedRoomId!)?.label}</p>
              </div>
            ) : (
              <div className="space-y-2">
                <p className="truncate"><strong>Student:</strong> {getUserForPlace(getSelectedPlace(moveOutState.selectedPlaceId!)!)?.firstName || 'Unknown'} {getUserForPlace(getSelectedPlace(moveOutState.selectedPlaceId!)!)?.lastName || 'Student'}</p>
                <p><strong>Current Place:</strong> Place {getSelectedPlace(moveOutState.selectedPlaceId!)?.index}</p>
              </div>
            )}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowConfirmDialog(false)}>
              Cancel
            </Button>
            <Button 
              onClick={activeTab === 'move-in' ? handleMoveInConfirm : handleMoveOutConfirm}
              disabled={isMovingIn || isMovingOut}
              variant={activeTab === 'move-in' ? 'default' : 'destructive'}
            >
              {(isMovingIn || isMovingOut) ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Processing...
                </>
              ) : (
                `Confirm ${activeTab === 'move-in' ? 'Move In' : 'Move Out'}`
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
} 