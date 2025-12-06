# Role Assignment Guide

## How to Assign Roles to Users

### Default Behavior
- **New registrations** are automatically assigned the **"Attendee"** role
- This is the default role for all new users

### How to Change User Roles

#### Option 1: Using Admin Panel (Recommended)
1. **Login as Admin**:
   - Email: `admin@example.com`
   - Password: `Admin@123`

2. **Navigate to Admin Panel**:
   - Click on "Manage Users" in the navigation menu (visible only to Admins)
   - Or go to: `/Admin/Users`

3. **Edit User Roles**:
   - Find the user you want to modify
   - Click "Edit Roles" button
   - Check/uncheck the roles you want to assign:
     - **Admin**: Full system access
     - **Organizer**: Can create and manage events
     - **Attendee**: Can purchase tickets (default role)
   - Click "Save Changes"

#### Option 2: Using Database Directly
If you need to assign roles directly in the database:

1. Find the user's ID in the `AspNetUsers` table
2. Find the role ID in the `AspNetRoles` table
3. Insert a record in `AspNetUserRoles` table:
   ```sql
   INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
   VALUES ('user-id-here', 'role-id-here');
   ```

### Available Roles

1. **Admin**
   - Full system access
   - Can manage users and roles
   - Can create/edit/delete all events
   - Can manage categories
   - Access to analytics

2. **Organizer**
   - Can create and manage their own events
   - Can view analytics for their events
   - Cannot manage other users
   - Cannot manage categories

3. **Attendee** (Default)
   - Can browse events
   - Can purchase tickets
   - Can view their own profile
   - Cannot create events

### Testing Different Roles

To test the system with different roles:

1. **Register a new user** (will be Attendee by default)
2. **Login as Admin** (`admin@example.com` / `Admin@123`)
3. **Go to Manage Users** and assign the desired role
4. **Logout and login** as the test user to see role-specific features

### Quick Role Assignment Examples

**Make a user an Organizer:**
- Login as Admin → Manage Users → Find user → Edit Roles → Check "Organizer" → Save

**Make a user an Admin:**
- Login as Admin → Manage Users → Find user → Edit Roles → Check "Admin" → Save

**Remove all roles:**
- Login as Admin → Manage Users → Find user → Edit Roles → Uncheck all → Save

